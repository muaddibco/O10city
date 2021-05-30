namespace O10.Client.DataLayer.AttributesScheme
{
    public static class AttributesSchemes
    {
        public const string ATTR_SCHEME_NAME_FIRSTNAME = "FirstName";
        public const string ATTR_SCHEME_NAME_LASTNAME = "LastName";
        public const string ATTR_SCHEME_NAME_IDCARD = "IdCard";
        public const string ATTR_SCHEME_NAME_DRIVINGLICENSE = "DrivingLicense";
        public const string ATTR_SCHEME_NAME_PASSPORT = "Passport";
        public const string ATTR_SCHEME_NAME_PLACEOFBIRTH = "PlaceOfBirth";
        public const string ATTR_SCHEME_NAME_DATEOFBIRTH = "DateOfBirth";
        public const string ATTR_SCHEME_NAME_PASSPORTPHOTO = "PassportPhoto";
        public const string ATTR_SCHEME_NAME_EMPLOYEEGROUP = "EmployeeGroup";
        public const string ATTR_SCHEME_NAME_EMAIL = "Email";
        public const string ATTR_SCHEME_NAME_ISSUER = "Issuer";
        public const string ATTR_SCHEME_NAME_NATIONALITY = "Nationality";
        public const string ATTR_SCHEME_NAME_PASSWORD = "Password";
        public const string ATTR_SCHEME_NAME_MISC = "Misc";
        public const string ATTR_SCHEME_NAME_ISSUANCEDATE = "IssuanceDate";
        public const string ATTR_SCHEME_NAME_EXPIRATIONDATE = "ExpirationDate";
        public const string ATTR_SCHEME_NAME_DL_VEHICLETYPE = "DlVehicleType";

        public const string ATTR_SCHEME_NAME_FIRSTNAME_DESC = "First Name";
        public const string ATTR_SCHEME_NAME_LASTNAME_DESC = "Last Name";
        public const string ATTR_SCHEME_NAME_IDCARD_DESC = "Id Card";
        public const string ATTR_SCHEME_NAME_DRIVINGLICENSE_DESC = "Driving License";
        public const string ATTR_SCHEME_NAME_PASSPORT_DESC = "Passport";
        public const string ATTR_SCHEME_NAME_PLACEOFBIRTH_DESC = "Place Of Birth";
        public const string ATTR_SCHEME_NAME_DATEOFBIRTH_DESC = "Date Of Birth";
        public const string ATTR_SCHEME_NAME_PASSPORTPHOTO_DESC = "Passport Photo";
        public const string ATTR_SCHEME_NAME_EMPLOYEEGROUP_DESC = "Employee Group";
        public const string ATTR_SCHEME_NAME_EMAIL_DESC = "Email";
        public const string ATTR_SCHEME_NAME_ISSUER_DESC = "Issuer";
        public const string ATTR_SCHEME_NAME_NATIONALITY_DESC = "Nationality";
        public const string ATTR_SCHEME_NAME_PASSWORD_DESC = "Password";
        public const string ATTR_SCHEME_NAME_MISC_DESC = "Miscellaneous";
        public const string ATTR_SCHEME_NAME_ISSUANCEDATE_DESC = "Issuance Date";
        public const string ATTR_SCHEME_NAME_EXPIRATIONDATE_DESC = "Expiration Date";
        public const string ATTR_SCHEME_NAME_DL_VEHICLETYPE_DESC = "Driving License Vehicle Type";

        public static AttributeScheme[] AttributeSchemes = new AttributeScheme[]
        {
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_MISC,
                Description = ATTR_SCHEME_NAME_MISC_DESC,
                ValueType = AttributeValueType.Any,
                IsMultiple = true
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_IDCARD,
                Description = ATTR_SCHEME_NAME_IDCARD_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_DRIVINGLICENSE,
                Description = ATTR_SCHEME_NAME_DRIVINGLICENSE_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_PASSPORT,
                Description = ATTR_SCHEME_NAME_PASSPORT_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_PLACEOFBIRTH,
                Description = ATTR_SCHEME_NAME_PLACEOFBIRTH_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_DATEOFBIRTH,
                Description = ATTR_SCHEME_NAME_DATEOFBIRTH_DESC,
                ValueType = AttributeValueType.Date
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_FIRSTNAME,
                Description = ATTR_SCHEME_NAME_FIRSTNAME_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_LASTNAME,
                Description = ATTR_SCHEME_NAME_LASTNAME_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_PASSPORTPHOTO,
                Description = ATTR_SCHEME_NAME_PASSPORTPHOTO_DESC,
                ValueType = AttributeValueType.Image
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_EMPLOYEEGROUP,
                Description = ATTR_SCHEME_NAME_EMPLOYEEGROUP_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_EMAIL,
                Description = ATTR_SCHEME_NAME_EMAIL_DESC,
                ValueType = AttributeValueType.Email
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_ISSUER,
                Description = ATTR_SCHEME_NAME_ISSUER_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_NATIONALITY,
                Description = ATTR_SCHEME_NAME_NATIONALITY_DESC,
                ValueType = AttributeValueType.Any
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_PASSWORD,
                Description = ATTR_SCHEME_NAME_PASSWORD_DESC,
                ValueType = AttributeValueType.Password
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_ISSUANCEDATE,
                Description = ATTR_SCHEME_NAME_ISSUANCEDATE_DESC,
                ValueType = AttributeValueType.Date
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_EXPIRATIONDATE,
                Description = ATTR_SCHEME_NAME_EXPIRATIONDATE_DESC,
                ValueType = AttributeValueType.Date
            },
            new AttributeScheme
            {
                Name = ATTR_SCHEME_NAME_DL_VEHICLETYPE,
                Description = ATTR_SCHEME_NAME_DL_VEHICLETYPE_DESC,
                ValueType = AttributeValueType.Any
            }
        };
    }
}
