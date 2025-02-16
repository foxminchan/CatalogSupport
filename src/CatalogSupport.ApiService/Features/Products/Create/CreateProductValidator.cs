using System.Net.Mime;

namespace CatalogSupport.ApiService.Features.Products.Create;

internal sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    private const int MaxFileSize = 1048576;

    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.CategoryId).NotEmpty();

        When(
            IsHasFiles,
            () =>
            {
                RuleFor(x => x.Image!)
                    .ChildRules(image =>
                    {
                        image
                            .RuleFor(x => x.Length)
                            .LessThanOrEqualTo(MaxFileSize)
                            .WithMessage(
                                $"The file size should not exceed {MaxFileSize / 1024} KB."
                            );
                        image
                            .RuleFor(x => x.ContentType)
                            .Must(x => x is MediaTypeNames.Image.Jpeg or MediaTypeNames.Image.Png)
                            .WithMessage(
                                "File type is not allowed. Allowed file types are JPEG and PNG."
                            );
                    });
            }
        );
    }

    private static bool IsHasFiles(CreateProductCommand command) => command.Image is not null;
}
