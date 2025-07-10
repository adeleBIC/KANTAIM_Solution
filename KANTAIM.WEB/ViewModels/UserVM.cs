using MudBlazor;
using KANTAIM.DAL.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KANTAIM.WEB.ViewModels
{
    public class UserVM : IValidatableObject
    {
        public static explicit operator User(UserVM vm) => vm.model; 
        private User model;

        public UserVM(IEnumerable<UserAccessLvl> userLvls) : this(new User(), userLvls) { }
        public UserVM(User model, IEnumerable<UserAccessLvl> userLvls)
        {
            this.model = model;
            this.UserLvls = userLvls.ToList();

            loginADUser = model.LoginADUser;
            userAccessLvlId = model.UserAccessLvlId;
            darkMode = model.DarkMode;
            comment = model.Comment;
        }

        public List<UserAccessLvl> UserLvls { get; set; }

        public bool IsEditing { get; set; }
        public bool IsChecked { get; set; }

        public int Id => model.Id;

        [Required]
        [Label("Nom")]
        private string loginADUser;
        public string LoginADUser
        {
            get { return loginADUser; }
            set { loginADUser = value; IsEditing = true; }
        }

        [Label("Dark Mode")]
        private bool darkMode;
        public bool DarkMode
        {
            get { return darkMode; }
            set { darkMode = value; IsEditing = true; }
        }

        [Label("Commentaire")]
        private string? comment;
        public string? Comment
        {
            get { return comment; }
            set { comment = value; IsEditing = true; }
        }


        [Label("Niveau d'accès")]
        private int userAccessLvlId;
        public int UserAccessLvlId
        {
            get { return userAccessLvlId; }
            set { userAccessLvlId = value; IsEditing = true; }
        }

        [Label("Niveau d'accès")]
        public string UserAccesslvlName { get => model.UserAccessLvl.Name; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> list = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(LoginADUser)) list.Add(new ValidationResult("Le nom est obligatoire", new string[] { "LoginADUser" }));
            if (UserAccessLvlId == 0) list.Add(new ValidationResult("Niveau obligatoire", new string[] { "UserAccessLvlId" }));

            if (list.Count <= 0)
            {
                model.LoginADUser = loginADUser;
                model.UserAccessLvlId = userAccessLvlId;
                model.DarkMode = darkMode;
                model.Comment = comment;
            }

            return list;
        }
    }
}
