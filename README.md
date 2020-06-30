# Code Sample - Active Directory
### Overview
>The code sample contains operations that can be performed on Active Directory. It also contains a NUnit Test project to test these operations.

### Operations
>* Create User
>* Remove User
>* Does User Exist
>* Change User Attribute
>The project holds overload methods if the LDAP string differs from the ones provided. 

### Tests
>* Create User Test
>* Remove User Test
>* Change User Attribute Test

### Optional Additions
>Above are some of the operations that can be performed on Active Directory along with other operations like, Move a user to an Organizational Unit, Add/Remove a user from a security group, Disable/Enable a users account and many more. 

### Requirements
>Running the test project will require a valid login to Active Directory on the targeted domain. This can be editted the the appSettings.config file.

### Other Notes
>Due to dummy values provided for login details, the test project will fail.
