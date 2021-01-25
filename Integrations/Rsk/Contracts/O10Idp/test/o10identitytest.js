const Web3 = require("web3")

const O10Identity = artifacts.require("O10Identity");

contract("setScheme", (accounts) => {
  // it("set scheme with 1 definition", async () => {
  //   console.log("Accounts:")
  //   console.log(accounts);

  //   let instance = await O10Identity.deployed();

  //   console.log("Registering issuer MOI")
  //   let txReg = await instance.register("MOI")
  //   console.log(txReg);

  //   console.log("Setting scheme for MOI")
  //   let txSch = await instance.setScheme([{"AttributeName":"CertificateNumber","Alias":"Certificate Number","AttributeScheme":"DrivingLicense","IsRoot":true}])
  //   console.log(txSch);

  //   let txAllIss = await instance.getAllIssuers();
  //   console.log(txAllIss);

  //   console.log("Getting scheme for MOI for " + accounts[0])
  //   let txSch2 = await instance.getScheme(accounts[0])
  //   console.log(txSch2);

  //   console.log("Issuing Attributes by MOI")
  //   let assetCommitment = attrStringToBytes("4ACBCD306265DFC2499C4415A1CFC898B206443FB0EE0D7F29CAE81C33BA01C1")
  //   let originatingCommitment = attrStringToBytes("6B3C02C0913065AFC215F9AA81B9F37AFC96FD17839257329A22AE3BA2E559A4")
  //   let txIss = await instance.issueAttributes([{"AttributeName":"CertificateNumber","AssetCommitment":assetCommitment,"BindingCommitment":originatingCommitment, "Version":0,"AttributeId":0}])
  //   console.log(txIss);

  //   let txCheck = await instance.checkRootAttributeValid(accounts[0], assetCommitment)
  //   console.log(txCheck)

  //   let assetCommitment2 = attrStringToBytes("0CEA97EA4597E32FDE998D7A27DE7308F342E5D18CCE6E1EE2B4DFDBA32CA676")
  //   let txIss2 = await instance.issueAttributes([{"AttributeName":"CertificateNumber","AssetCommitment":assetCommitment2,"BindingCommitment":originatingCommitment, "Version":0,"AttributeId":0}])
  //   console.log(txIss2);

  //   let txCheck2 = await instance.checkRootAttributeValid(accounts[0], assetCommitment)
  //   console.log(txCheck2)

  //   let txCheck3 = await instance.checkRootAttributeValid(accounts[0], assetCommitment2)
  //   console.log(txCheck3)
  // });

  it("set scheme with 4 definition", async () => {
    console.log("Accounts:")
    console.log(accounts);

    let instance = await O10Identity.deployed();

    console.log("Registering issuer MOI2")
    let txReg = await instance.register("MOI2")
    console.log(txReg);

    console.log("Setting scheme for MOI2")
    let txSch = await instance.setScheme(
      [
        {"AttributeName":"IDCard","Alias":"ID Card","AttributeScheme":"IdCard","IsRoot":true},
        {"AttributeName":"FirstName","Alias":"First Name","AttributeScheme":"FirstName","IsRoot":false},
        {"AttributeName":"LastName","Alias":"Last Name","AttributeScheme":"LastName","IsRoot":false},
        {"AttributeName":"Password","Alias":"Password","AttributeScheme":"Password","IsRoot":false}
      ])
    console.log(txSch);

    let txAllIss = await instance.getAllIssuers();
    console.log(txAllIss);

    console.log("Getting scheme for MOI2 for " + accounts[0])
    let txSch2 = await instance.getScheme(accounts[0])
    console.log(txSch2);

    console.log("Issuing Attributes by MOI")
    let idCardCommitment = attrStringToBytes("8B79B9C07799B436BBA659474D6238FADD7A785A53BCE45C96BB1FF197D70D88")
    let idCardBinding = attrStringToBytes("81EF040D46DF2D34ED16710132C4E6EC841309B27F9FAA9D4268FDBD8359F223")

    let firstNameCommitment = attrStringToBytes("F02D41EEE767C26FC1384DC81987C24162D018475D844405C23B948E2EC7EC7C")
    let firstNameBinding = attrStringToBytes("613D1FDBE1A8C2D8BE112665A41F154B39D30D4F25B89425FD1158BEE5002294")

    let lastNameCommitment = attrStringToBytes("7A3B2CAFACBF1BE576EDFE03A621F2FAD1A29D13649C423CFCE1AD79FC71492D")
    let lastNameBinding = attrStringToBytes("B4BC84CE197275F6EE6DDFE9EE65FD738C134D41ACD1A8B0C30BF6C7ABE992C0")

    let passwordCommitment = attrStringToBytes("405C70B12614715B6F969F182B03AA0215F7709549C31256465C31CF9C15D517")
    let passwordBinding = attrStringToBytes("2125633082FA5C3E24894FA29AF9AD135B31E966C44C84609F270FCC3E15D979")

    let txIss = await instance.issueAttributes([
      {"AttributeName":"IDCard","AssetCommitment":idCardCommitment,"BindingCommitment":idCardBinding, "Version":0,"AttributeId":0},
      {"AttributeName":"FirstName","AssetCommitment":firstNameCommitment,"BindingCommitment":firstNameBinding, "Version":0,"AttributeId":0},
      {"AttributeName":"LastName","AssetCommitment":lastNameCommitment,"BindingCommitment":lastNameBinding, "Version":0,"AttributeId":0},
      {"AttributeName":"Password","AssetCommitment":passwordCommitment,"BindingCommitment":passwordBinding, "Version":0,"AttributeId":0}
    ])
    console.log(txIss);

    let txCheck = await instance.checkRootAttributeValid(accounts[0], idCardCommitment)
    console.log(txCheck)

    let idCardCommitment2 = attrStringToBytes("0CEA97EA4597E32FDE998D7A27DE7308F342E5D18CCE6E1EE2B4DFDBA32CA676")

    let txIss2 = await instance.issueAttributes([
      {"AttributeName":"IDCard","AssetCommitment":idCardCommitment2,"BindingCommitment":idCardBinding, "Version":0,"AttributeId":0}
    ])
    console.log(txIss2);

    let txCheck2 = await instance.checkRootAttributeValid(accounts[0], idCardCommitment)
    console.log(txCheck2)

    let txCheck3 = await instance.checkRootAttributeValid(accounts[0], idCardCommitment2)
    console.log(txCheck3)
  });
});


function attrStringToBytes(str) {
  if (str.length !== 64) {
    throw new Error('invalid attribute string');
  }
  const hexStr = '0x' + str;
  return Web3.utils.hexToBytes(hexStr);
}
