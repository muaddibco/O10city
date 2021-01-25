import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class ServiceProviderFrontService {
	constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

	getServiceProviders() {
		return this.http.get<ServiceProviderInfoDto[]>(this.baseUrl + "api/ServiceProviders/GetAll");
	}

	getServiceProvidersForSession(scenarioId: string) {
		return this.http.get<ServiceProviderInfoDto[]>(this.baseUrl + "api/ServiceProviders/GetAll?scenarioId=" + scenarioId);
	}

	getServiceProvider(id: string) {
		return this.http.get<ServiceProviderInfoDto>(this.baseUrl + "api/ServiceProviders/ById/" + id);
	}
	getSessionInfo(id: string) {
		return this.http.get<ServiceProviderSessionDto>(this.baseUrl + "api/SpUsers/GetSessionInfo/" + id);
	}

	getSpDocuments(id: string) {
		return this.http.get<SpDocument[]>("/api/SpUsers/GetDocuments/" + id);
	}

	getSpDocumentSignatures(id: string) {
		return this.http.get<SpDocument[]>("/api/SpUsers/DocumentSignatures/" + id);
	}

	getServiceProviderRelationGroups() {
		return this.http.get<ServiceProviderRelationGroups[]>("/api/SchemaResolution/ServiceProviderRelationGroups");
	}

	addSpDocument(id: string, document: SpDocument) {
		return this.http.post<SpDocument>("/api/SpUsers/AddDocument/" + id, document);
	}

	deleteSpDocument(id: string, documentId: number) {
		return this.http.delete("/api/SpUsers/DeleteDocument/" + id + "/api/" + documentId.toString());
	}

	addAllowedSigner(id: string, documentId: number, allowedSigner: AllowedSigner) {
		return this.http.post<AllowedSigner>("/api/SpUsers/AddAllowedSigner/" + id + "/api/" + documentId.toString(), allowedSigner);
	}

	deleteAllowedSigner(id: string, allowedSignerId: number) {
		return this.http.delete("/api/SpUsers/DeleteAllowedSigner/" + id + "/api/" + allowedSignerId.toString());
	}

	getRegistrationsAndTransactions(accountId: string) {
		return this.http.get<ServiceProviderRegistrationDto[]>('/ServiceProviders/UserTransaction?accountId=' + accountId);
	}

	getRegistrationAndTransactions(accountId: string, registrationId: number) {
		return this.http.get<ServiceProviderRegistrationDto>('/ServiceProviders/UserTransaction/' + registrationId + '?accountId=' + accountId);
	}

	pushUserTransaction(accountId: string, registrationId: string, description: string) {
		return this.http.post<SpUserTransaction>('/ServiceProviders/UserTransaction?accountId=' + accountId, { registrationId, description });
	}
}

export interface ServiceProviderSessionDto {
  publicKey: string;
  sessionKey: string;
}

export interface ServiceProviderInfoDto {
  id: string;
  description: string;
  target: string;
}

export interface RelationGroup {
    name: string;
}

export interface ServiceProviderRelationGroups {
    publicSpendKey: string,
    publicViewKey: string,
    alias: string,
    description: string,
    relationGroups: RelationGroup[]
}


export interface SpDocument {
    documentId: number;
    documentName: string;
    hash: string;
    allowedSigners: AllowedSigner[];
    signatures: SpDocumentSignature[];
}

export interface SpDocumentSignature {
    documentId: number;
    signatureId: number;
    documentHash: string;
    documentRecordHeight: number;
    signatureRecordHeight: number;
    signatureVerification: DocumentSignatureVerification;
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

export interface AllowedSigner {
    allowedSignerId: number;
    groupOwner: string;
    groupName: string;
}

export interface ServiceProviderRegistrationDto {
	serviceProviderRegistrationId: string;
	commitment: string;
	transactions: SpUserTransaction[];
}

export interface SpUserTransaction {
	spUserTransactionId: number;
	registrationId: number;
	transactionId: string;
	description: string;
	isProcessed: boolean;
	isConfirmed: boolean;
	isCompromised: boolean;
}
