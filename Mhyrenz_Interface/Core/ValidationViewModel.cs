using Mhyrenz_Interface.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mhyrenz_Interface.Core
{
    public class ValidationViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _propertyErrors = new Dictionary<string, List<string>>();

        public BaseAsyncCommand SubmitActionCommand { get; set; }

        public Dictionary<string, List<string>> PropertyErrors => _propertyErrors;

        public bool HasErrors => _propertyErrors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public Action ClearValidations { get; set; }

        public event EventHandler<ProductDataViewModel> SubmitSuccess;



        public void RaiseSubmitSuccess(ProductDataViewModel product)
        {
            SubmitSuccess?.Invoke(this, product);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return Enumerable.Empty<string>();

            _propertyErrors.TryGetValue(propertyName, out var error);
            return error ?? Enumerable.Empty<string>();
        }

        protected void Validate(string propertyName, object propertyValue)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(propertyValue, new ValidationContext(this) { MemberName = propertyName }, results);


            if (results.Any())
            {
                _propertyErrors[propertyName] = results.Select(r => r.ErrorMessage).ToList();
            }
            else
            {
                _propertyErrors.Remove(propertyName);
            }

            OnErrorsChanged(propertyName);
            SubmitActionCommand.OnCanExecuteChanged();
            Validator.TryValidateObject(this, new ValidationContext(this), null, validateAllProperties: true);
        }

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public virtual void InvokeClearValidations()
        {
            var propertyNames = PropertyErrors.Keys.ToList();
            PropertyErrors.Clear();

            foreach (var propertyName in propertyNames)
            {
                OnErrorsChanged(propertyName);
            }
        }
    }
}
