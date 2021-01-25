const HDWalletProvider = require('@truffle/hdwallet-provider');
const privkey = "0x4312e19b2abb4d47667eb4cc44d21cc38fb1d28b4a426a3c94daad214c6c63dc"
const PrivateKeyProvider = require("truffle-privatekey-provider");

const fs = require('fs');
const mnemonic = fs.readFileSync(".secret").toString().trim();
if (!mnemonic || mnemonic.split(' ').length !== 12) {
  throw new Error('unable to retrieve mnemonic from .secret');
}

const gasPriceTestnetRaw = fs.readFileSync(".gas-price-testnet.json").toString().trim();
const gasPriceTestnet = parseInt(JSON.parse(gasPriceTestnetRaw).result, 16);
if (typeof gasPriceTestnet !== 'number' || isNaN(gasPriceTestnet)) {
  throw new Error('unable to retrieve network gas price from .gas-price-testnet.json');
}
console.log("Gas price Testnet: " + gasPriceTestnet);

const path = require("path");

module.exports = {
  networks: {
    testnet: {
      provider: () => new HDWalletProvider(mnemonic, 'https://public-node.testnet.rsk.co/2.0.1/'),
      network_id: 31,
      gasPrice: Math.floor(gasPriceTestnet * 1.1),
      networkCheckTimeout: 1e9
    },
    testdebugnet: {
      provider: () => new PrivateKeyProvider(privkey, 'https://public-node.testnet.rsk.co/2.0.1/'),
      network_id: 31,
      gasPrice: Math.floor(gasPriceTestnet * 1.1),
      networkCheckTimeout: 1e9
    },
    ganache: {
      host: "127.0.0.1",
      port: 7545,
      network_id: "*" // Match any network id
    },
    regtest: {
      host: "127.0.0.1",
      port: 4444,
      network_id: 33 // Match any network id
    }
  },  

  contracts_build_directory: path.join(__dirname, "app/src/contracts"),

  compilers: {
    solc: {
      version: "0.6.2",
    }
  }
}