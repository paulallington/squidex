﻿// ==========================================================================
//  ContentValidationTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core
{
    public class ContentValidationTests
    {
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.DE, Language.EN);
        private readonly List<ValidationError> errors = new List<ValidationError>();
        private Schema schema = Schema.Create("my-name", new SchemaProperties());

        [Fact]
        public async Task Should_add_error_if_validating_data_with_unknown_field()
        {
            var data =
                new NamedContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown is not a known field", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_field()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant, new NumberFieldProperties { MaxValue = 100 }));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .SetValue(1000));

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field must be less than '100'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_non_localizable_data_field_contains_language()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported invariant value 'es'", "my-field"),
                    new ValidationError("my-field has an unsupported invariant value 'it'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_data_with_invalid_localizable_field()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Language, new NumberFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData();

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field (de) is required", "my-field"),
                    new ValidationError("my-field (en) is required", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_required_data_field_is_not_in_bag()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant, new NumberFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData();

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field is required", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_data_contains_invalid_language()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Language));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("xx", 1));

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported language value 'xx'", "my-field")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_required_field_has_no_value_for_optional_language()
        {
            var optionalConfig =
                LanguagesConfig.Create(Language.ES, Language.IT).Update(Language.IT, true, false, null);

            schema = schema.AddOrUpdateField(new StringField(1, "my-field", Partitioning.Language, new StringFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", "value"));

            await data.ValidateAsync(schema, optionalConfig.ToResolver(), errors);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_add_error_if_data_contains_unsupported_language()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Language));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidateAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported language value 'es'", "my-field"),
                    new ValidationError("my-field has an unsupported language value 'it'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_unknown_field()
        {
            var data =
                new NamedContentData()
                    .AddField("unknown",
                        new ContentFieldData());

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("unknown is not a known field", "unknown")
                });
        }

        [Fact]
        public async Task Should_add_error_if_validating_partial_data_with_invalid_field()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant, new NumberFieldProperties { MaxValue = 100 }));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .SetValue(1000));

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field must be less than '100'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_non_localizable_partial_data_field_contains_language()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported invariant value 'es'", "my-field"),
                    new ValidationError("my-field has an unsupported invariant value 'it'", "my-field")
                });
        }

        [Fact]
        public async Task Should_not_add_error_if_validating_partial_data_with_invalid_localizable_field()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Language, new NumberFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData();

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_not_add_error_if_required_partial_data_field_is_not_in_bag()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant, new NumberFieldProperties { IsRequired = true }));

            var data =
                new NamedContentData();

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_add_error_if_partial_data_contains_invalid_language()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Language));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("xx", 1));

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported language value 'xx'", "my-field")
                });
        }

        [Fact]
        public async Task Should_add_error_if_partial_data_contains_unsupported_language()
        {
            schema = schema.AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Language));

            var data =
                new NamedContentData()
                    .AddField("my-field",
                        new ContentFieldData()
                            .AddValue("es", 1)
                            .AddValue("it", 1));

            await data.ValidatePartialAsync(schema, languagesConfig.ToResolver(), errors);

            errors.ShouldBeEquivalentTo(
                new List<ValidationError>
                {
                    new ValidationError("my-field has an unsupported language value 'es'", "my-field"),
                    new ValidationError("my-field has an unsupported language value 'it'", "my-field")
                });
        }
    }
}
