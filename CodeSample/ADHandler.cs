using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using CodeSample.Models;
using CodeSample.Responses;
using System.Configuration;
using System.Collections.Specialized;

namespace CodeSample
{
    public class ADHandler
    {
          static protected NameValueCollection Config
          {
              get
              {
                return (NameValueCollection)ConfigurationManager.GetSection("appSettings");
              }
          }
          protected string BASE_OU_INACTIVE = Config["InactiveOUPath"];
          protected string BASE_OU_ACTIVE = Config["ActiveOUPath"];
          protected string DEFAULT_SSOUSERSID_PROPERTY = "extensionAttribute1";

          private String _SSOUsersIDProperty;
          public String SSOUsersIDProperty
          {
            get { return (String.IsNullOrWhiteSpace(_SSOUsersIDProperty) ? DEFAULT_SSOUSERSID_PROPERTY : _SSOUsersIDProperty); }
            set { _SSOUsersIDProperty = value; }
          }

          public static String DEFAULT_DOMAIN_CONTROLLER = "Company.007";

          private String _DomainController;
          public String DomainController
          {
            get { return (String.IsNullOrWhiteSpace(_DomainController) ? DEFAULT_DOMAIN_CONTROLLER : _DomainController); }
            set { _DomainController = value; }
          }

    /// <summary>
    /// If user does not exist, this method will create a user in the one of the pre-defined Organizational Unit.
    /// </summary>
    /// <param name="s">User information</param>
    /// <param name="path">Pre-defined OUPaths</param>
    /// <param name="login">Login information to Active Directory.</param>
    /// <returns>ObjectGUID (within BaseResponse) created by Active Directory.</returns>
    public BaseResponse<Guid> CreateUser(UserInfoModel s, OUPath path)
    {
      return CreateUser(s, LDAPString(path, s));
    }

    /// <summary>
    /// If user does not exist, this method will create a user in the specified Organizational Unit.
    /// </summary>
    /// <param name="s">User information</param>
    /// <param name="path">Distingushed Name of the Organizational Unit.</param>
    /// <param name="login">Login information to Active Directory.</param>
    /// <returns>ObjectGUID (within BaseResponse) created by Active Directory.</returns>
    public BaseResponse<Guid> CreateUser(UserInfoModel s, string LDAPPath)
    {
      ADResponseType rt = ADResponseType.Undefined;
      Guid rResult = Guid.Empty;
      string rMessage = string.Empty;
      Exception rException = null;
      ADLoginModel login = GetAdminActiveDirectoryLogin();
      try
      {
        if (!DoesUserExist(s))
        {
          using (var pc = new PrincipalContext(ContextType.Domain, DomainController, GetDomainContainer(LDAPPath), ContextOptions.SimpleBind, login.Username, login.Password))
          {
            using (var up = new UserPrincipal(pc))
            {
              up.SamAccountName = s.Username;
              up.GivenName = s.FirstName;
              up.Surname = s.LastName;
              up.MiddleName = s.MiddleName;
              up.EmailAddress = s.DomainEmailAddress;
              up.UserPrincipalName = s.DomainEmailAddress;
              up.DisplayName = String.Format("{0} {1}", s.FirstName, s.LastName);

              up.SetPassword(s.Password);
              up.PasswordNeverExpires = true;
              up.Enabled = true;

              up.Save();

              ((DirectoryEntry)up.GetUnderlyingObject()).Properties[SSOUsersIDProperty].Value = s.ID.ToString();
              up.Save();

              rResult = ((DirectoryEntry)up.GetUnderlyingObject()).Guid;
              rt = ADResponseType.OK;
            }
          }
        }
        else
        {
          rMessage = "User already exists.";
          rt = ADResponseType.Warning;
        }
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryOperationException E)
      {
        rMessage = "Unable to perform operation.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      catch (DirectoryServicesCOMException E)
      {
        Console.WriteLine(String.Format("EXCEPTION : {0}\r\n{1}", E.Message, E.StackTrace));
        rMessage = "Unable to set password.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException E)
      {
        rMessage = "There is a problem with the LDAP string.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectExistsException E)
      {
        rMessage = "User already exists.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      catch (System.Reflection.TargetInvocationException E)
      {
        rMessage = "Password does not meet requirements.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      return new BaseResponse<Guid>(rResult, rt, rMessage, rException);
    }
    /// <summary>
    /// This method will remove (delete) the user, if exists.  
    /// </summary>
    /// <param name="s">User information</param>
    /// <param name="login">Login information to Active Directory.</param>
    /// <returns>Deleted user's email address.</returns>
    public BaseResponse<string> RemoveUser(UserInfoModel s)
    {

      ADResponseType rt;
      string rResult = string.Empty;
      string rMessage = string.Empty;
      Exception rException = null;
      ADLoginModel login = GetAdminActiveDirectoryLogin();
      try
      {
        PrincipalContext pc = new PrincipalContext(ContextType.Domain, login.Domain, login.Username, login.Password);
        UserPrincipal user = UserPrincipal.FindByIdentity(pc, s.DomainEmailAddress);
        if (user != null)
        {
          user.Delete();
          rt = ADResponseType.OK;
          rResult = s.DomainEmailAddress;
        }
        else
        {
          rMessage = "User does not exist.";
          rt = ADResponseType.Undefined;
        }
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryOperationException E)
      {
        rMessage = "Unable to perform operation.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException E)
      {
        rMessage = "User was not found.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      return new BaseResponse<string>(rResult, rt, rMessage, rException);
    }

 

    /// <summary>
    /// Checks if specified User is part of specified Domain.
    /// </summary>
    /// <param name="s">User information</param>
    /// <param name="login">Login information to Active Directory.</param>
    /// <returns>(bool) True.</returns>
    public bool DoesUserExist(UserInfoModel s)
    {
      bool retVal = false;
      ADLoginModel login = GetAdminActiveDirectoryLogin();
      try
      {
        PrincipalContext pc = new PrincipalContext(ContextType.Domain, login.Domain, login.Username, login.Password);
        UserPrincipal user = UserPrincipal.FindByIdentity(pc, s.DomainEmailAddress);
        if (user != null)
        {
          retVal = true;
        }
        else
        {
          retVal = false;
        }
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryOperationException)
      {
        retVal = false;
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException)
      {
        retVal = false;
      }
      return retVal;
    }


    /// <summary>
    /// Changes an attribute of a user.
    /// </summary>
    /// <param name="s">User information</param>
    /// <param name="path">Pre-defined OUPath of user's Organizational Unit</param>
    /// <param name="AttributeName">Name for the attribute to be changed from a defined set of attributes.</param>
    /// <param name="AttributeValue">New attribute value.</param>
    /// <param name="login">Login information to Active Directory.</param>
    /// <returns>ResponseType.OK</returns>
    public BaseResponse<string> ChangeUserAttribute(UserInfoModel s, OUPath path, UserAttributes AttributeName, string AttributeValue)
    {
      return ChangeUserAttribute(s, LDAPString(path), AttributeName, AttributeValue);
    }

    /// <summary>
    /// Changes an attribute of a user.
    /// </summary>
    /// <param name="s">User information</param>
    /// <param name="path">Distingushed Name of the Organizational Unit containing user.</param>
    /// <param name="AttributeName">Name for the attribute to be changed from a defined set of attributes.</param>
    /// <param name="AttributeValue">New attribute value.</param>
    /// <param name="login">Login information to Active Directory.</param>
    /// <returns>ResponseType.OK</returns>
    public BaseResponse<string> ChangeUserAttribute(UserInfoModel s, string path, UserAttributes AttributeName, string AttributeValue)
    {
      ADResponseType rt;
      string rResult = "";
      string rMessage = "";
      Exception rException = null;
      ADLoginModel login = GetAdminActiveDirectoryLogin();
      try
      {
        DirectoryEntry dirEntry = new DirectoryEntry(path, login.Username, login.Password);
        if (dirEntry != null)
        {
          DirectorySearcher search = new DirectorySearcher(dirEntry);
          search.Filter = "(&(objectClass=user)(SAMAccountName=" + s.Username + "))";
          SearchResult result = search.FindOne();
          if (result != null)
          {
            DirectoryEntry userEntry = new DirectoryEntry(result.Path, login.Username, login.Password);
            if (userEntry != null)
            {
              if (userEntry.Properties.Contains("" + AttributeName.ToString() + ""))
              {
                userEntry.Properties["" + AttributeName.ToString() + ""].Value = AttributeValue;
                userEntry.CommitChanges();
                rt = ADResponseType.OK;
              }
              else
              {
                userEntry.Properties["" + AttributeName.ToString() + ""].Add(AttributeValue);
                userEntry.CommitChanges();
                rt = ADResponseType.OK;
              }
            }
            else
            {
              rMessage = "User does not exist.";
              rt = ADResponseType.Undefined;
            }
          }
          else
          {
            rMessage = "User does not exist.";
            rt = ADResponseType.Undefined;
          }
        }
        else
        {
          rMessage = "LDAP string is incorrect.";
          rt = ADResponseType.Warning;
        }
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectNotFoundException E)
      {
        rMessage = "Parameters passed in were incorrect";
        rt = ADResponseType.Exception;
        rException = E;
      }
      catch (System.DirectoryServices.ActiveDirectory.ActiveDirectoryOperationException E)
      {
        rMessage = "Unable to perform operation.";
        rt = ADResponseType.Exception;
        rException = E;
      }
      return new BaseResponse<string>(rResult, rt, rMessage, rException);

    }

    /// <summary>
    /// Pre-defined OUPaths
    /// </summary>
    public enum OUPath
    {
      Inactive,
      Students
    }

    public string GetDomainContainer(OUPath path, UserInfoModel ui = null)
    {
      return GetDomainContainer(LDAPString(path, ui));
    }

    public string GetDomainContainer(string path)
    {
      return path.Replace("LDAP://", "");
    }

    /// <summary>
    /// Gets the pre-defined LDAP Path
    /// </summary>
    /// <param name="ou">OUPath</param>
    /// <param name="prefixOU">string containing a prefix OU to prepend to the path</param>
    /// <returns>Gets the pre-defined LDAP Path</returns>
    public string LDAPString(OUPath ou, UserInfoModel ui)
    {
      if ((OUPath.Inactive == ou || OUPath.Students == ou) && (null != ui) && (!string.IsNullOrWhiteSpace(ui.LastName)))
      {
        return LDAPString(ou, ui.LastName.Substring(0, 1).ToUpper());
      }
      return LDAPString(ou);
    }

    /// <summary>
    /// Gets the pre-defined LDAP Path
    /// </summary>
    /// <param name="ou">OUPath</param>
    /// <param name="prefixOU">string containing a prefix OU to prepend to the path</param>
    /// <returns>Gets the pre-defined LDAP Path</returns>
    public string LDAPString(OUPath ou, string prefixOU = null)
    {
      string sRetVal = "";
      string sPrefix = (string.IsNullOrWhiteSpace(prefixOU) ? "" : string.Format("OU={0},", prefixOU));

      switch (ou)
      {
        case OUPath.Inactive:
          sRetVal = string.Format("LDAP://{0}{1}", sPrefix, BASE_OU_INACTIVE);
          break;
        case OUPath.Students:
          sRetVal = string.Format("LDAP://{0}{1}", sPrefix, BASE_OU_ACTIVE);
          break;
      }

      return sRetVal;
    }

    /// <summary>
    /// Gets Active directory login info - if does not exist.
    /// </summary>
    /// <param name="l">ADLogin class</param>
    /// <returns> Returns Active directory login info</returns>
    private ADLoginModel GetAdminActiveDirectoryLogin()
    {
      return new ADLoginModel()
      {
        Domain = Config["ADDomain"],
        Username = Config["ADUsername"],
        Password = Config["ADPassword"]
    };
    }
  }
}
