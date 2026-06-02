using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mhyrenz_Interface.Core
{
    public abstract class ValidationViewModel<T> : BaseViewModel, INotifyDataErrorInfo where T : class
    {

        public Dictionary<string, List<string>> PropertyErrors { get; } = new Dictionary<string, List<string>>();

        public bool HasErrors => PropertyErrors.Count != 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public event EventHandler<T> SubmitSuccess;

        protected abstract IRaiseCanExecuteChanged SubmitActionCommand();

        public void RaiseSubmitSuccess(T product)
        {
            SubmitSuccess?.Invoke(this, product);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return Enumerable.Empty<string>();

            PropertyErrors.TryGetValue(propertyName, out var error);
            return error ?? Enumerable.Empty<string>();
        }

        protected void Validate(string propertyName, object propertyValue)
        {
            ClearErrors(propertyName);

            ValidateDataAnnotations(propertyName, propertyValue);

            ValidateCustom(propertyName);

            OnErrorsChanged(propertyName);

            SubmitActionCommand().OnCanExecuteChanged();
        }

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearErrors(string propertyName)
        {
            PropertyErrors.Remove(propertyName);
        }

        private void ValidateDataAnnotations(
        string propertyName,
        object propertyValue)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(
                propertyValue,
                new ValidationContext(this)
                {
                    MemberName = propertyName
                },
                results);

            foreach (var result in results)
            {
                AddError(propertyName, result.ErrorMessage);
            }
        }

        protected virtual void ValidateCustom(string propertyName) { }

        protected void AddError(string propertyName, string error)
        {
            if (!PropertyErrors.TryGetValue(propertyName, out var errors))
            {
                errors = new List<string>();
                PropertyErrors[propertyName] = errors;
            }

            if (!errors.Contains(error))
                errors.Add(error);
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

        public override void Dispose()
        {
            ErrorsChanged = null;
            SubmitSuccess = null;
        }
    }
}
