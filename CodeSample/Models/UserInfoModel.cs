using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample.Models
{
  /// <summary>
  /// User details
  /// </summary>
  public class UserInfoModel : IObjectModel
  {
    public Guid ID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string Username { get; set; }
    public string DomainEmailAddress { get; set; }
    public string Password { get; set; }
    public string NewPassword { get; set; }
  }
}
