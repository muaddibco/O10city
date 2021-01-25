import { Injectable } from '@angular/core';
const Web3 = require('web3');

declare let require: any;
declare let window: any;
const tokenAbi = require('../../contracts/O10Identity.json');

@Injectable({
  providedIn: 'root'
})
export class O10IdentityService {

  private account: any = null;
  private readonly web3: any;
  private enable: any;
  
  constructor() {
    if (window.ethereum === undefined) {
      alert('Non-Ethereum browser detected. Install MetaMask');
    } else {
      if (typeof window.web3 !== 'undefined') {
        this.web3 = window.web3.currentProvider;
      } else {
        this.web3 = new Web3.providers.HttpProvider('http://localhost:8545');
      }
      console.log('o10identity.service :: constructor :: window.ethereum');
      window.web3 = new Web3(window.ethereum);
      console.log('o10identity.service :: constructor :: this.web3');
      console.log(this.web3);
      this.enable = this.enableMetaMaskAccount();
    }

    this.getAccount();
   }

   private async enableMetaMaskAccount(): Promise<any> {
    let enable = false;
    await new Promise((resolve, reject) => {
      enable = window.ethereum.enable();
    });
    return Promise.resolve(enable);
  }

  private async getAccount(): Promise<any> {
    console.log('o10identity.service :: getAccount :: start');
    if (this.account == null) {
      this.account = await new Promise((resolve, reject) => {
        console.log('o10identity.service :: getAccount :: eth');
        console.log(window.web3.eth);
        window.web3.eth.getAccounts((err, retAccount) => {
          console.log('o10identity.service :: getAccount: retAccount');
          console.log(retAccount);
          if (retAccount.length > 0) {
            this.account = retAccount[0];
            resolve(this.account);
          } else {
            alert('o10identity.service :: getAccount :: no accounts found.');
            reject('No accounts found.');
          }
          if (err != null) {
            alert('o10identity.service :: getAccount :: error retrieving account');
            reject('Error retrieving account');
          }
        });
      }) as Promise<any>;
    }
    return Promise.resolve(this.account);
  }

  public async getIssuers(): Promise<IssuerDetails[]> {
    const that = this;
    console.log('o10identity.service :: getIssuers :: start');
    return new Promise((resolve, reject) => {
      console.log('o10identity.service :: getIssuers :: tokenAbi');
      console.log(tokenAbi);
      const contract = require('@truffle/contract');
      const o10IdentityContract = contract(tokenAbi);
      o10IdentityContract.setProvider(that.web3);
      console.log('o10identity.service :: getIssuers :: o10IdentityContract');
      console.log(o10IdentityContract);
      o10IdentityContract.deployed().then(function(instance) {
        return instance.getAllIssuers(
          {
            from: that.account
          });
      }).then(function(status) {
        if (status) {
          console.log(status);
          return resolve(<IssuerDetails[]>status);
        }
      }).catch(function(error) {
        console.log(error);
        return reject('o10identity.service error');
      });
    });
  }

  public async registerIssuer(issuerAlias: string): Promise<any> {
    const that = this;
    console.log('o10identity.service :: registerIssuer :: start');
    return new Promise((resolve, reject) => {
      console.log('o10identity.service :: registerIssuer :: tokenAbi');
      console.log(tokenAbi);
      const contract = require('@truffle/contract');
      const o10IdentityContract = contract(tokenAbi);
      o10IdentityContract.setProvider(that.web3);
      console.log('o10identity.service :: registerIssuer :: o10IdentityContract');
      console.log(o10IdentityContract);
      o10IdentityContract.deployed().then(function(instance) {
        console.log('o10identity.service :: registerIssuer :: invoke');
        return instance.register(
          issuerAlias,
          {
            from: that.account
          });
      }).then(function(status) {
        console.log('o10identity.service :: registerIssuer :: status', status);
        if (status) {
          console.log(status);
          return resolve();
        }
      }).catch(function(error) {
        console.error('o10identity.service :: registerIssuer :: error', error);
        return reject('o10identity.service error');
      });
    });
  }

  public async setScheme(definitions: AttributeDefinition[]): Promise<any> {
    const that = this;
    console.log('o10identity.service :: setScheme :: start');
    return new Promise((resolve, reject) => {
      console.log('o10identity.service :: setScheme :: tokenAbi');
      console.log(tokenAbi);
      const contract = require('@truffle/contract');
      const o10IdentityContract = contract(tokenAbi);
      o10IdentityContract.setProvider(that.web3);
      console.log('o10identity.service :: setScheme :: o10IdentityContract');
      console.log(o10IdentityContract);
      o10IdentityContract.deployed().then(function(instance) {
        console.log('o10identity.service :: setScheme :: invoke');
        return instance.setScheme(
          definitions,
          {
            from: that.account
          });
      }).then(function(status) {
        console.log('o10identity.service :: setScheme :: status', status);
        if (status) {
          console.log(status);
          return resolve();
        }
      }).catch(function(error) {
        console.error('o10identity.service :: setScheme :: error', error);
        return reject('o10identity.service error');
      });
    });
  }

  public async getScheme(addr: string): Promise<AttributeDefinition[]> {
    const that = this;
    console.log('o10identity.service :: getScheme :: start');
    return new Promise((resolve, reject) => {
      console.log('o10identity.service :: getScheme :: tokenAbi');
      console.log(tokenAbi);
      const contract = require('@truffle/contract');
      const o10IdentityContract = contract(tokenAbi);
      o10IdentityContract.setProvider(that.web3);
      console.log('o10identity.service :: getScheme :: o10IdentityContract');
      console.log(o10IdentityContract);
      o10IdentityContract.deployed().then(function(instance) {
        return instance.getScheme(
          addr,
          {
            from: that.account
          });
      }).then(function(status) {
        if (status) {
          console.log(status);
          return resolve((<GetSchemeResponse>status).AttributeDefinitions);
        }
      }).catch(function(error) {
        console.log(error);
        return reject('o10identity.service error');
      });
    });
  }
}

export interface IssuerDetails {
  Alias: string;
  Address: string;
}

export class AttributeDefinition {
  AttributeName: string;
  AttributeScheme: string;
  Alias: string;
  IsRoot: boolean;
}

export interface GetSchemeResponse {
  Version: number;
  AttributeDefinitions: AttributeDefinition[];
}
