# Abstract

Biometric Proof required for proving that provided cryptographic proofs were created using Root
Attribute that has registration at some Biometric Verifier and Biometric Verifier verified
prover at the time of proofs creation.

It can be that receiver of cryptographic proofs will require proving that biometric proof
associated with photo on some document.

## What is biometric proof?

A User can register at various Biometric Verifiers. Depending on whether biometric factor
associated with the Root Attribute or one bounded to Associated Root Attribute commitment is
calculated differently.

- ***[ C<sub>a<sup>b</sup><sub>i</sub></sub> = r<sub>a<sup>b</sup><sub>i</sub></sub>G + I<sub>x</sub> ]*** - in a case when a biometric commitment is bounded to a Root Attribute
- ***[ C<sub>a<sup>b</sup><sub>i</sub></sub> = r<sub>a<sup>b</sup><sub>i</sub></sub>G + I<sub>a<sup>r</sup><sub>i</sub></sub> + I<sub>x</sub> ]*** - in a case when a biometric commitment is bounded to an Associated Root Attribute

### Registration phase
Thus, when a User wants to register proofs of inherence factor that is Associated Attribute she
will need demonstrate Surjection Proofs that will include both - Root and Associated Root
Commitments. As a result, it will be clear that a User has both inherence factors - Root and
Associated. This will oblige her to bring proof to Root inherence factor.

### Proving phase
Quite the same at the Proving Phase - a user will need to bring proves for both, Root and
Associated inherence factors.

## Open questions

### Matching Root to Associated

There must be possibility to prove that main biometric proof matches Associated Attribute. This
means that the User has main biometric proof that is stored at the level of Issuer of her Root
Attribute. 

