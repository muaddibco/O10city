var o10Identity = artifacts.require("O10Identity");

 module.exports = function(deployer) {
   // deployment steps
   deployer.deploy(o10Identity, "O10 Identity Provider", "O10IdP");
 };

 // 0xdced172F1c976F7B83CCd25deb9A261639c01642
 // 0x2A868205D57C247028391aD231eD9417189b0E9E