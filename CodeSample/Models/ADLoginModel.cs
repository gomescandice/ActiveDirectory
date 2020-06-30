using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample.Models
{
  /// <summary>
  /// Active directory login details.
  /// </summary>
  public class ADLoginModel : IObjectModel
  {
    public string Username;
    public string Password;
    public string Domain;
  }
}
