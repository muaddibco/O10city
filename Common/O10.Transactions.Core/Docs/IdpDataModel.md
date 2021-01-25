Every IdP will have a set of one or more Attributes where one Attribute is the Root and others are Associated with it. 
Root Attribute, in its turn, can be bounded to a Root Attribute of another IdP. In this case it is not issued to the owner but just registered in the Network as any other Associated Attribute.

Every attribute will be stored as a tuple of two commitments:
 - ***[ C<sub>a<sup>v</sup><sub>i</sub></sub> = r<sub>a<sup>v</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub> ]*** - a commitment to the Attribute value.
 - Either one of following commitments, depending whether this is Root Attribute of the IdP or Associated one: 
   - ***[ C<sub>a<sup>r</sup><sub>i</sub></sub> = r<sub>a<sup>r</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub> + I<sub>a<sup>v</sup><sub>r</sub></sub> ]*** - a commitment to the Associated Attribute Value and to the parent Root Attribute Value. Required for proving association binding of the Attribute to the parent Root Attribute.
   - ***[ C<sub>a<sup>r</sup><sub>i</sub></sub> = r<sub>a<sup>r</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub> + I<sub>x</sub> ]*** - a commitment to the Root Attribute Value and to the Root Attribute Value from another IdP. Required for proving association binding of the Root Attribute to the Root Attribute of another IdP.

Here:

- ***r<sub>a<sup>v</sup><sub>i</sub></sub>*** = Hash(pwd) + Hash( value of parent)
- ***r<sub>a<sup>r</sup><sub>i</sub></sub>*** = Hash(pwd) + Hash( value of parent | value of attr )

### Proofs at Transaction

When the User will want to prove authenticity of her Attribute value she'll need to issue a transaction containing the following details:
- ***[ C<sub>x</sub><sup><sub>1</sub></sup> = r<sub>x</sub><sup><sub>1</sub></sup>G + I<sub>x</sub> ]*** - a commitment to the Root Attribute of the User.
- ***[ C<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup> = r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>i</sub></sub> ]*** - a commitment to the Associated Attribute value.
- ***[ C<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> = r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> ]*** - a commitment to the Root Attribute that Associated Attribute bounded to. Omitted in the case when the Associated Attribute is Root one at the IdP and bounded right to Root Attribute of the User.

Now, in order to prove that the User is able to deliver an Associated Attribute bounded to the 
Root Attribute, owned by the User, she just need to sign the transaction with some Secret Key so others will 
able to verify this signature using accessible Public Key. This is called Surjection Proof.
Depending on whether User need to deliver proof for Attribute that is Root or not calculation of Surjection Proof can be one of the following:

#### For Attribute that is Root at IdP

##### Inputs

At the IdP:
- ***C<sub>a<sup>v</sup><sub>r</sub></sub> = r<sub>a<sup>v</sup><sub>r</sub></sub>G + I<sub>a<sup>v</sup><sub>r</sub></sub>***
- ***C<sub>a<sup>r</sup><sub>r</sub></sub> = r<sub>a<sup>r</sup><sub>r</sub></sub>G + I<sub>a<sup>v</sup><sub>r</sub></sub> + I<sub>x</sub>***

At the transaction:
- ***C<sub>x</sub><sup><sub>1</sub></sup> = r<sub>x</sub><sup><sub>1</sub></sup>G + I<sub>x</sub>***
- ***C<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> = r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub>***

##### Proof of knowledge of the Attribute Value

***C<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> - C<sub>a<sup>v</sup><sub>r</sub></sub> 
= (r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> ) - (r<sub>a<sup>v</sup><sub>r</sub></sub>G + I<sub>a<sup>v</sup><sub>r</sub></sub> )
= r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> - r<sub>a<sup>v</sup><sub>r</sub></sub>G - I<sub>a<sup>v</sup><sub>r</sub></sub>
= (r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> - r<sub>a<sup>v</sup><sub>r</sub></sub> )G***

##### Proof of knowledge of associated with the Root Attribute

***C<sub>x</sub><sup><sub>1</sub></sup> + C<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> - C<sub>a<sup>r</sup><sub>r</sub></sub>
= (r<sub>x</sub><sup><sub>1</sub></sup>G + I<sub>x</sub> ) + (r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> ) - (r<sub>a<sup>r</sup><sub>r</sub></sub>G + I<sub>a<sup>v</sup><sub>r</sub></sub> + I<sub>x</sub> )
= r<sub>x</sub><sup><sub>1</sub></sup>G + I<sub>x</sub> + r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> - r<sub>a<sup>r</sup><sub>r</sub></sub>G - I<sub>a<sup>v</sup><sub>r</sub></sub> - I<sub>x</sub>
= (r<sub>x</sub><sup><sub>1</sub></sup> + r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> - r<sub>a<sup>r</sup><sub>r</sub></sub> )G***


#### For Attribute that is not Root at IdP

##### Inputs

At the IdP:
- ***C<sub>a<sup>v</sup><sub>r</sub></sub> = r<sub>a<sup>v</sup><sub>r</sub></sub>G + I<sub>a<sup>v</sup><sub>r</sub></sub>***
- ***C<sub>a<sup>r</sup><sub>r</sub></sub> = r<sub>a<sup>r</sup><sub>r</sub></sub>G + I<sub>a<sup>v</sup><sub>r</sub></sub> + I<sub>x</sub>***
- ***C<sub>a<sup>v</sup><sub>i</sub></sub> = r<sub>a<sup>v</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub>***
- ***C<sub>a<sup>r</sup><sub>i</sub></sub> = r<sub>a<sup>r</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub> + I<sub>a<sup>v</sup><sub>r</sub></sub>***

At the transaction:
- ***C<sub>x</sub><sup><sub>1</sub></sup> = r<sub>x</sub><sup><sub>1</sub></sup>G + I<sub>x</sub>***
- ***C<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> = r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub>***
- ***C<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup> = r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>i</sub></sub>***

##### Proof of knowledge of the Attribute Value

***C<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup> - C<sub>a<sup>v</sup><sub>i</sub></sub> 
= (r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>i</sub></sub> ) - (r<sub>a<sup>v</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub> )
= r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>i</sub></sub> - r<sub>a<sup>v</sup><sub>i</sub></sub>G - I<sub>a<sup>v</sup><sub>i</sub></sub>
= (r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup> - r<sub>a<sup>v</sup><sub>i</sub></sub> )G***

##### Proof of knowledge of associated with the Root Attribute

***C<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> + C<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup> - C<sub>a<sup>r</sup><sub>i</sub></sub>
= (r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> ) + (r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>i</sub></sub> ) - (r<sub>a<sup>r</sup><sub>i</sub></sub>G + I<sub>a<sup>v</sup><sub>i</sub></sub> + I<sub>a<sup>v</sup><sub>r</sub></sub> )
= r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>r</sub></sub> + r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup>G + I<sub>a<sup>v</sup><sub>i</sub></sub> - r<sub>a<sup>r</sup><sub>i</sub></sub>G - I<sub>a<sup>v</sup><sub>i</sub></sub> - I<sub>a<sup>v</sup><sub>r</sub></sub>
= (r<sub>a<sup>v</sup><sub>r</sub></sub><sup><sub>1</sub></sup> + r<sub>a<sup>v</sup><sub>i</sub></sub><sup><sub>1</sub></sup> - r<sub>a<sup>r</sup><sub>i</sub></sub> )G***
