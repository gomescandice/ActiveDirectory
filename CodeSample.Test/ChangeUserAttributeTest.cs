using System;
using Xunit;
using CodeSample;
using CodeSample.Models;

namespace CodeSampleTest
{
  public class ChangeUserAttributeTest
  {
    [Fact]
    public void ChangeAttribute()
    {
      string newMobileNo = "92899901880";
      UserInfoModel user = new UserInfoModel()
      {
        Username = "john.doe",
        DomainEmailAddress = "john.doe@company.com"
      };
      //Act
      ADHandler AD = new ADHandler();

      var result = AD.ChangeUserAttribute(user, ADHandler.OUPath.Students, UserAttributes.mobile, newMobileNo);

      //Assert
      Assert.True(result.responseType == CodeSample.Responses.ADResponseType.OK);
    }
  }
}
