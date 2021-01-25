import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class IdentitiesService {
    constructor(private http: HttpClient) { }

  addIdentity(accountId: number, identity: IdentityDto) {
    return this.http.post<any>('/api/IdentityProvider/Identity?accountId=' + accountId, identity);
    }

    getIdentity(id: string) {
        return this.http.get<IdentityDto>('/api/IdentityProvider/GetIdentityById/' + id);
    }

  getIdentityAttributesSchema(accountId: number) {
    return this.http.get<IdentityAttributesSchemaDto>('/api/IdentityProvider/AttributesSchema?accountId=' + accountId);
    }

    getSchemeItems() {
        return this.http.get<SchemeItem[]>('/api/SchemaResolution/SchemeItems');
    }

    getAttributeDefinitions(issuer: string) {
        return this.http.get<AttributeDefinition[]>('/api/SchemaResolution/AttributeDefinitions?issuer=' + issuer);
    }

    saveScheme(issuer: string, attributeDefinitions: AttributeDefinition[]) {
        return this.http.put<AttributeDefinition[]>('/api/SchemaResolution/AttributeDefinitions?issuer=' + issuer, attributeDefinitions);
    }
}

export interface IdentityDto {
  numberOfTransfers: number;
  id: string;
  description: string;
  attributes: IdentityAttributeDto[];
}

export interface IdentityAttributeDto {
  attributeName: string;
  content: string;
  originatingCommitment: string;
}

export interface IdentityAttributeSchemaDto {
  name: string;
  schemeName: string;
}

export interface IdentityAttributesSchemaDto {
  rootAttribute: IdentityAttributeSchemaDto;
  associatedAttributes: IdentityAttributeSchemaDto[];
}

export interface SchemeItem {
    name: string;
    description: string;
    valueType: string;
    allowMultiple: boolean;
}

export interface AttributeDefinition {
    schemeId: number;
    attributeName: string;
    schemeName: string;
    alias: string;
    description: string;
    isActive: boolean;
    isRoot: boolean;
}
