import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { access } from 'fs';

@Injectable()
export class ServiceProviderService {
	constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

	getServiceProvider(id: number) {
		return this.http.get<ServiceProviderInfoDto>('/api/ServiceProviders/ById/' + id);
	}

	getRegistrations(accountId: number) {
		return this.http.get<ServiceProviderRegistrationDto[]>('/api/ServiceProviders/Registrations?accountId=' + accountId);
	}

	getRegistrationsAndTransactions(accountId: number) {
		return this.http.get<ServiceProviderRegistrationDto[]>('/api/ServiceProviders/UserTransaction?accountId=' + accountId);
	}

	getRegistrationAndTransactions(accountId: number, registrationId: number) {
		return this.http.get<ServiceProviderRegistrationDto>('/api/ServiceProviders/UserTransaction/' + registrationId + '?accountId=' + accountId);
	}

	pushUserTransaction(accountId: number, registrationId: string, description: string) {
		return this.http.post<SpUserTransaction>('/api/ServiceProviders/UserTransaction?accountId=' + accountId, { registrationId, description });
	}

	getIdentityAttributeValidationDescriptors(accountId: number) {
		return this.http.get<IdentityAttributeValidationDescriptorDto[]>('/api/ServiceProviders/IdentityAttributeValidationDescriptors?accountId=' + accountId);
	}

	getIdentityAttributeValidations(accountId: number) {
		return this.http.get<IdentityAttributeValidationDefinitionDto[]>('/api/ServiceProviders/IdentityAttributeValidations?accountId=' + accountId);
	}

	saveIdentityAttributeValidations(accountId: number, identityAttributeValidationDefinitions: IdentityAttributeValidationDefinitionDto[]) {
		return this.http.post<any>('/api/ServiceProviders/IdentityAttributeValidationDefinitions?accountId=' + accountId, { identityAttributeValidationDefinitions });
	}

	getEmployeeGroups(accountId: number) {
		return this.http.get<EmployeeGroup[]>('/api/ServiceProviders/EmployeeGroups?accountId=' + accountId);
	}

	addEmployeeGroup(accountId: number, employeeGroup: EmployeeGroup) {
		return this.http.post<EmployeeGroup>('/api/ServiceProviders/EmployeeGroup?accountId=' + accountId, employeeGroup);
	}

	deleteEmployeeGroup(accountId: number, groupId: number) {
		return this.http.delete('/api/ServiceProviders/EmployeeGroup?accountId=' + accountId + '&groupId=' + groupId.toString());
	}

	getEmployees(accountId: number) {
		return this.http.get<EmployeeRecord[]>('/api/ServiceProviders/Employees?accountId=' + accountId);
	}

	addEmployee(accountId: number, employee: EmployeeRecord) {
		return this.http.put<EmployeeRecord>('/api/ServiceProviders/Employee?accountId=' + accountId, employee);
	}

	updateEmployee(accountId: number, employee: EmployeeRecord) {
		return this.http.post<EmployeeRecord>('/api/ServiceProviders/Employee?accountId=' + accountId, employee);
	}

	deleteEmployee(accountId: number, employeeId: number) {
		return this.http.delete('/api/ServiceProviders/Employee?accountId=' + accountId + '&employeeId=' + employeeId.toString());
	}

	getSpDocuments(accountId: number) {
		return this.http.get<SpDocument[]>('/api/ServiceProviders/GetDocuments?accountId=' + accountId);
	}

	addSpDocument(accountId: number, document: SpDocument) {
		return this.http.post<SpDocument>('/api/ServiceProviders/Document?accountId=' + accountId, document);
	}

	deleteSpDocument(accountId: number, documentId: number) {
		return this.http.delete('/api/ServiceProviders/Document?accountId=' + accountId + '&documentId=' + documentId.toString());
	}

	addAllowedSigner(accountId: number, documentId: number, allowedSigner: AllowedSigner) {
		return this.http.post<AllowedSigner>('/api/ServiceProviders/AllowedSigner?accountId=' + accountId + '&documentId=' + documentId.toString(), allowedSigner);
	}

	deleteAllowedSigner(accountId: number, allowedSignerId: number) {
		return this.http.delete('/api/ServiceProviders/AllowedSigner?accountId=' + accountId + '&allowedSignerId=' + allowedSignerId.toString());
	}

	getServiceProviderRelationGroups() {
		return this.http.get<ServiceProviderRelationGroups[]>("/SchemaResolution/ServiceProviderRelationGroups");
	}
}

export interface ServiceProviderRegistrationDto {
	serviceProviderRegistrationId: string;
	commitment: string;
	userTransactions: SpUserTransaction[];
}

export interface SpAttributeDto {
	schemeName: string;
	source: string;
	assetId: string;
	originalBlindingFactor: string;
	originalCommitment: string;
	issuingCommitment: string;
	validated: boolean;
	content: string;
	isOverriden: boolean;
}

export interface IdentityAttributeValidationDescriptorDto {
	schemeName: string;
	schemeAlias: string;
	validationType: string;
	validationTypeName: string;
	validationCriterionTypes: string[];
}

export interface IdentityAttributeValidationDefinitionDto {
	schemeName: string;
	validationType: string;
	criterionValue: string;
}

export class IdentityAttributeValidationDefinitionClassDto implements IdentityAttributeValidationDefinitionDto {
	schemeName: string;
	schemeAlias: string;
	validationType: string;
	validationTypeName: string;
	criterionValue: string;
}

export interface ServiceProviderInfoDto {
	id: string;
	description: string;
	target: string;
}

export interface EmployeeGroup {
	groupId: number;
	groupName: string;
}

export interface EmployeeRecord {
	employeeId: number;
	description: string;
	rawRootAttribute: string;
	assetId: string;
	registrationCommitment: string;
	groupId: number;
}

export interface AllowedSigner {
	allowedSignerId: number;
	groupOwner: string;
	groupName: string;
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

export interface SpUserTransaction {
	spUserTransactionId: number;
	registrationId: number;
	transactionId: string;
	description: string;
	isProcessed: boolean;
	isConfirmed: boolean;
	isCompromised: boolean;
}
