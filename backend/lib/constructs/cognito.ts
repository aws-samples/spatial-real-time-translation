import { CfnOutput, Duration, RemovalPolicy, Stack } from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as cognito from 'aws-cdk-lib/aws-cognito';
import * as iam from 'aws-cdk-lib/aws-iam';
import { NagSuppressions } from 'cdk-nag';

export type CognitoConstructProps = {
	identityPoolPolicies?: { [name: string]: iam.PolicyDocument };
};

export class CognitoConstruct extends Construct {
	public readonly userPool: cognito.UserPool;
	public readonly userPoolClient: cognito.UserPoolClient;

	/**
	 * @param scope the construct scope.
	 * @param id the identifier given the construct.
	 * @param props the construct configuration.
	 */
	constructor(scope: Construct, id: string, props: CognitoConstructProps) {
		super(scope, id);

		this.userPool = new cognito.UserPool(this, 'UserPool', {
			selfSignUpEnabled: true,
			userVerification: {
				emailStyle: cognito.VerificationEmailStyle.LINK
			},
			passwordPolicy: {
				minLength: 12,
				requireLowercase: true,
				requireUppercase: true,
				requireDigits: true,
				requireSymbols: true,
				tempPasswordValidity: Duration.days(3)
			},
			autoVerify: {
				email: true
			},
			accountRecovery: cognito.AccountRecovery.EMAIL_ONLY,
			removalPolicy: RemovalPolicy.DESTROY
		});

		const cfnUserPool = this.userPool.node.defaultChild as cognito.CfnUserPool;
		cfnUserPool.userPoolAddOns = {
			advancedSecurityMode: 'ENFORCED'
		};

		this.userPoolClient = this.userPool.addClient('Client', {
			oAuth: {
				flows: {
					implicitCodeGrant: true
				},
				scopes: [cognito.OAuthScope.OPENID, cognito.OAuthScope.EMAIL, cognito.OAuthScope.PHONE, cognito.OAuthScope.PROFILE, cognito.OAuthScope.COGNITO_ADMIN]
			},
			authFlows: {
				userSrp: true,
				userPassword: true
			},
			supportedIdentityProviders: [cognito.UserPoolClientIdentityProvider.COGNITO]
		});

		this.userPool.addDomain('Domain', {
			cognitoDomain: {
				domainPrefix: process.env.APP_STACK_NAME ?? 'vr-realtime-translate'
			}
		});

		const identityPool = new cognito.CfnIdentityPool(this, 'IdentityPool', {
			allowUnauthenticatedIdentities: false,
			cognitoIdentityProviders: [
				{
					clientId: this.userPoolClient.userPoolClientId,
					providerName: this.userPool.userPoolProviderName
				}
			]
		});

		const translationPolicy = new iam.PolicyDocument({
			statements: [
				new iam.PolicyStatement({
					effect: iam.Effect.ALLOW,
					actions: [
						'transcribe:StartStreamTranscriptionWebSocket',
						'translate:TranslateText',
						'polly:SynthesizeSpeech'
					],
					resources: ['*']
				})
			]
		});

		let inlinePolicies: { [name: string]: iam.PolicyDocument } = {
			translationPolicy: translationPolicy
		};

		if (props.identityPoolPolicies != undefined) {
			inlinePolicies = {
				translationPolicy: translationPolicy,
				...props.identityPoolPolicies
			};
		}

		const authenticatedRole = new iam.Role(this, 'users-group-role', {
			description: 'Default role for authenticated users',
			assumedBy: new iam.FederatedPrincipal(
				'cognito-identity.amazonaws.com',
				{
					StringEquals: {
						'cognito-identity.amazonaws.com:aud': identityPool.ref
					},
					'ForAnyValue:StringLike': {
						'cognito-identity.amazonaws.com:amr': 'authenticated'
					}
				},
				'sts:AssumeRoleWithWebIdentity'
			),
			inlinePolicies: inlinePolicies
		});

		const anonymousRole = new iam.Role(this, 'anonymous-group-role', {
			description: 'Default role for anonymous users',
			assumedBy: new iam.FederatedPrincipal(
				'cognito-identity.amazonaws.com',
				{
					StringEquals: {
						'cognito-identity.amazonaws.com:aud': identityPool.ref
					},
					'ForAnyValue:StringLike': {
						'cognito-identity.amazonaws.com:amr': 'unauthenticated'
					}
				},
				'sts:AssumeRoleWithWebIdentity'
			),
			inlinePolicies: inlinePolicies
		});

		new cognito.CfnIdentityPoolRoleAttachment(this, 'identity-pool-role-attachment', {
			identityPoolId: identityPool.ref,
			roles: {
				authenticated: authenticatedRole.roleArn,
				unauthenticated: anonymousRole.roleArn
			},
			roleMappings: {
				mapping: {
					type: 'Token',
					ambiguousRoleResolution: 'AuthenticatedRole',
					identityProvider: `cognito-idp.${Stack.of(this).region}.amazonaws.com/${this.userPool.userPoolId}:${this.userPoolClient.userPoolClientId}`
				}
			}
		});

		new CfnOutput(this, 'CognitoUserPoolId', {
			value: this.userPool.userPoolId
		});

		new CfnOutput(this, 'CognitoClientId', {
			value: this.userPoolClient.userPoolClientId
		});

		new CfnOutput(this, 'IdentityPoolId', {
			value: identityPool.ref
		});

		//#region CDK-Nag Suppressions
		// allow unauthenticated user access
		NagSuppressions.addResourceSuppressions(identityPool, [
			{
				id: 'AwsSolutions-COG7:',
				reason: 'unauthenticated login required for initial account setup'
			}
		]);

		// allow wildcard in authenticatedRole
		NagSuppressions.addResourceSuppressions(authenticatedRole, [
			{
				id: 'AwsSolutions-IAM5',
				reason: 'translationPolicy resource left open to allow dynamic access'
			}
		]);

		// allow wildcard in anonymousRole
		NagSuppressions.addResourceSuppressions(anonymousRole, [
			{
				id: 'AwsSolutions-IAM5',
				reason: 'translationPolicy resource left open to allow dynamic access'
			}
		]);
		//#endregion
	}
}
