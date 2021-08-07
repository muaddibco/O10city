import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class UserService {

	constructor(private http: HttpClient) { }

	getUserDetails(accountId: number) {
    return this.http.get<IUser>('/api/User/UserDetails?accountId=' + accountId);
	}

  getUserAttributes(accountId: number) {
    return this.http.get<UserAttributeDto[]>('/api/User/UserAttributes?accountId=' + accountId)
	}

  duplicateUser(sourceAccountId: number, accountInfo: string) {
    return this.http.post<any>('/api/Accounts/DuplicateUserAccount', { sourceAccountId, accountInfo });
	}

  overrideUserAccount(accountId: number, password: string, secretSpendKey: string, secretViewKey: string, lastCombinedBlockHeight: number) {
    return this.http.put<any>('/api/Accounts/UserAccount?accountId=' + accountId, { password, secretSpendKey, secretViewKey, lastCombinedBlockHeight });
	}

  requestIdentity(accountId: number, target: string, idCardContent: string, password: string, imageContent: string) {
    return this.http.post<any>('/api/User/RequestForIdentity?accountId=' + accountId, { target, idCardContent, password, imageContent });
	}

  identityRegistration(accountId: number, target: string, idCardContent: string, passphrase: string, password: string, imageContent: string) {
    return this.http.post<any>('/api/User/IdentityRegistration?accountId=' + accountId, { target, idCardContent, passphrase, password, imageContent });
	}

  getUserAssociatedAttributes(accountId: number, issuer: string) {
		return this.http.get<UserAssociatedAttributeDto[]>('/api/User/UserAssociatedAttributes?accountId=' + accountId + '&issuer=' + issuer);
	}

  updateUserAssociatedAttributes(accountId: number, issuer: string, attrs: UserAssociatedAttributeDto[]) {
		return this.http.post<any>('/api/User/UserAssociatedAttributes?accountId=' + accountId + '&issuer=' + issuer, attrs);
  }

  setUserRootAttributeContent(accountId: number, userAttribute: UserAttributeDto) {
    return this.http.post('/api/User/UserRootAttribute?accountId=' + accountId, userAttribute);
  }

  getActionType(target: string) {
		return this.http.get<ActionTypeDto>('/api/User/ActionType?actionInfo=' + target);
	}

  sendCompromisedProofs(accountId: number, unauthorizedUse: UnauthorizedUseDto) {
    return this.http.post<any>('/api/User/CompromisedProofs?accountId=' + accountId, unauthorizedUse);
	}

  clearCompromised(accountId: number) {
    return this.http.post<any>('/api/User/ClearCompromised?accountId=' + accountId, null);
	}

  getDocumentSignatureVerification(documentCreator: string, documentHash: string, documentRecordHeight: string, signatureRecordBlockHeight: string) {
		return this.http.get<DocumentSignatureVerification>('/api/User/DocumentSignatureVerification?documentCreator=' + documentCreator + "&documentHash=" + documentHash + "&documentRecordHeight=" + documentRecordHeight + "&signatureRecordBlockHeight=" + signatureRecordBlockHeight);
	}

  getGroupRelations(accountId: number) {
    return this.http.get<GroupRelation[]>('/api/User/GroupRelations?accountId=' + accountId);
	}

	getUserRegistrations(accountId: number) {
		return this.http.get<UserRegistration[]>('/api/User/UserRegistrations?accountId=' + accountId);
	}

  deleteGroupRelation(groupRelationId: number) {
		return this.http.delete('/api/User/GroupRelation/' + groupRelationId.toString());
	}

  sendRelationProofs(accountId: number, relationProofs: any) {
		return this.http.post('/api/User/RelationsProofs?accountId=' + accountId, relationProofs);
	}

  deleteUserAttribute(accountId: number, attributeId: number) {
    return this.http.delete('/api/User/UserRootAttribute?accountId=' + accountId + '&attributeId=' + attributeId.toString());
	}

  getSchemeItems() {
		return this.http.get<SchemeItem[]>('/api/SchemaResolution/SchemeItems');
	}

  getServiceProviderActionType(actionInfo: string) {
    return this.http.get<ActionType>('/api/User/ServiceProviderActionType?actionInfo=' + actionInfo);
	}

  getServiceProviderActionInfo(accountId: number, actionInfo: string, assetId: string, attributeContent: string) {
		return this.http.get<ServiceProviderActionAndValidations>('/api/User/ActionDetails?accountId=' + accountId + '&actionInfo=' + actionInfo + "&assetId=" + assetId + "&attributeContent=" + attributeContent);
	}

  getDisclosedSecrets(accountId: number, password: string) {
		return this.http.get<DiscloseSecretsQR>('/api/User/DiscloseSecrets?accountId=' + accountId + '&password=' + password);
	}

  challengeProofs(key: string, proofsRequest: ProofsRequest) {
		return this.http.post<ProofsSessionKey>('/api/User/ChallengeProofs?key=' + key, proofsRequest);
	}

  resetCompromisedAccount(accountId: number, password: string) {
    return this.http.get<any>('/api/Accounts/ResetCompromisedAccount?accountId=' + accountId + '&password=' + password);
  }

  stopAccount(accountId: number) {
    return this.http.put<any>('/api/Accounts/StopAccount/' + accountId, null);
  }

  getIsPhotoRequired(target: string) {
    return this.http.get<IdentityRequestDefinitions>('/api/User/PhotoRequired?target=' + target);
  }
}

export class ActionType {
	public static IdentityRequest: string = '1';
	public static ServiceProvider: string = '2';
	public static ProofRelations: string = '3';
  public static RegisterIdentity: string = '4';
	public static ValidateSignature: string = '5';
  public static OverrideAccount: string = '6';
  public static Saml: string = '7';
}

export class SpActionType {
	public static LoginRegister: string = '0';
	public static Relation: string = '1';
	public static DocumentSign: string = '2';
	public static Saml: string = '3';
	public static Consent: string = '4';
}

export interface DiscloseSecretsQR {
	qr: string;
}

export interface UserAssociatedAttributeDto {
    schemeName: string;
    alias: string;
    content: string;
    valueType: string;
}

export interface ActionTypeDto {
    action: string;
    actionInfo: string;
}


export class UserAttributeDto {
    userAttributeId: number;
    schemeName: string;
    source: string;
    issuerName: string;
    assetId: string;
    originalBlindingFactor: string;
    originalCommitment: string;
    lastBlindingFactor: string;
    lastCommitment: string;
    lastTransactionKey: string;
    lastDestinationKey: string;
    validated: boolean;
    content: string;
	  dirtyContent: string;
	  isOverriden: boolean;
}

export class UserAttributeLastUpdateDto {
    issuer: string;
    assetId: string;
    lastBlindingFactor: string;
    lastCommitment: string;
    lastTransactionKey: string;
    lastDestinationKey: string;
}

export class UserAttributeTransferDto extends UserAttributeDto {
    target: string;
    target2: string;
    payload: string;
    imageContent: string;
    extraInfo: string;
    password: string;
}

export class UserAttributeTransferWithValidationsDto {
    userAttributeTransfer: UserAttributeTransferDto;
    password: string;
}

export interface UnauthorizedUseDto {
    keyImage: string;
    transactionKey: string;
    target: string;
}

export class User implements IUser {

    publicViewKey: string;

    publicSpendKey: string;

    id: string;

    accountInfo: string;

	isCompromised: boolean;

	isAutoTheftProtection: boolean;

	consentManagementHub: string;
}

export interface IUser {

    publicViewKey: string;

    publicSpendKey: string;

    id: string;

    accountInfo: string;

	isCompromised: boolean;

	isAutoTheftProtection: boolean;
	consentManagementHub: string;
}

export interface DocumentSignatureVerification {
  isNotCompromised: boolean;
  signatureTransactionFound: boolean;
  documentRecordTransactionFound: boolean;
  documentHashMatch: boolean;
  signerSignatureMatch: boolean;
  eligibilityCorrect: boolean;
  allowedGroupRelation: boolean;
  allowedGroupMatching: boolean;
}

export interface GroupRelation {
    groupRelationId: number;
    groupOwnerName: string;
    groupOwnerKey: string;
    groupName: string;
    issuer: string;
    assetId: string;
}

export interface RelationsProofs {
    targetSpendKey: string;
    targetViewKey: string
    imageContent: string;
    relations: GroupRelation[];
}

export interface ProofsRequest {
	withKnowledgeProof: boolean;
	withBiometricProof: boolean;
}

export interface RelationProofsValidationResults {
	imageContent: string;
	isImageCorrect: boolean;
	isEligibilityCorrect: boolean;
	isKnowledgeFactorCorrect: boolean;
  validationResults: RelationProofValidationResult[];
  isCompromised: boolean;
}

export interface RelationProofValidationResult {
	relatedAttributeOwner: string;
	relatedAttributeContent: string;
	isRelationCorrect: boolean;
}

export interface SchemeItem {
	name: string;
	description: string;
	valueType: string;
	allowMultiple: boolean;
}

export interface ServiceProviderActionAndValidations {
	isRegistered: boolean;
	publicKey: string;
	publicKey2: string;
	sessionKey: string;
	isBiomteryRequired: boolean;
	extraInfo: string;
	predefinedAttributeId: number;
	validations: string[];
}

export interface ProofsChallenge {
	key: string;
	publicSpendKey: string;
	publicViewKey: string;
	sessionKey: string;
	withKnowledgeProof: boolean;
	withBiometricProof: boolean;
}

export interface ProofsSessionKey {
	sessionKey: string;
}

export interface IdentityRequestDefinitions {
  isPhotoRequired: boolean;
}

export interface UserRegistration {
	userRegistrationId: string;
	commitment: string;
	assetId: string;
	issuer: string;
}

export interface ConsentRequest {
	registrationCommitment: string;
	transactionId: string;
	description: string;
	publicSpendKey: string;
	publicViewKey: string;
}

export interface ActionType {
  actionType: string;
}
