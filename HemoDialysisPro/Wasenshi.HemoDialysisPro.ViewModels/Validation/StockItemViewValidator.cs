using FluentValidation;

namespace Wasenshi.HemoDialysisPro.ViewModels.Validation
{
    public class StockItemViewValidator : AbstractValidator<StockItemViewModel>
    {
        public StockItemViewValidator()
        {
            RuleFor(x => x.EntryDate).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.UnitId).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty();
        }
    }
}
