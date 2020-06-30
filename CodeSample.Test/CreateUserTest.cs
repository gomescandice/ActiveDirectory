using System;
using Xunit;
using CodeSample;
using CodeSample.Models;

namespace CodeSampleTest
{
  public class CreateUserTest
  {
    [Fact]
    public void CreateUser()
    {
      UserInfoModel user = new UserInfoModel()
      {
        ID = Guid.NewGuid(),
        FirstName = "John",
        LastName = "Doe",
        MiddleName = "Henry",
        Username = "john.doe",
        Password = "12345Abcde!",
        DomainEmailAddress = "john.doe@company.com"
      };
      //Act
      ADHandler AD = new ADHandler();
      var result = AD.CreateUser(user, ADHandler.OUPath.Students);
      Guid output = Guid.Empty;
      //Assert
      Assert.True(result.responseType == CodeSample.Responses.ADResponseType.OK);
    }
  }
}
