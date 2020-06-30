using System;
using Xunit;
using CodeSample;
using CodeSample.Models;

namespace CodeSampleTest
{
  public class RemoveUserTest
  {
    [Fact]
    public void RemoveUser()
    {
      UserInfoModel user = new UserInfoModel()
      {
        DomainEmailAddress = "john.doe@company.com"
      };
      //Act
      ADHandler AD = new ADHandler();
      var result = AD.RemoveUser(user);

      //Assert
      Assert.True(result.responseType == CodeSample.Responses.ADResponseType.OK);
    }
  }
}
