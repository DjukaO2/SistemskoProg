using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sistemsko
{
    public class TopicModeling
    {
        private readonly MLContext _mlContext;

        public TopicModeling()
        {
            _mlContext = new MLContext();
        }

        public List<TransformedTextData> PerformTopicModeling(List<string> descriptions)
        {
            var data = descriptions.Select(d => new TextData { Text = PreprocessText(d) }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Text.NormalizeText("NormalizedText", "Text")
                            .Append(_mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "NormalizedText"))
                            .Append(_mlContext.Transforms.Text.ProduceWordBags("Features", "Tokens"))
                            .Append(_mlContext.Transforms.Text.LatentDirichletAllocation("LdaFeatures", "Features", numberOfTopics: 5));

            var model = pipeline.Fit(dataView);
            var transformedData = model.Transform(dataView);

            var transformedDataEnumerable = _mlContext.Data.CreateEnumerable<TransformedTextData>(transformedData, reuseRowObject: false).ToList();

            
            for (int i = 0; i < transformedDataEnumerable.Count; i++)
            {
                var row = transformedDataEnumerable[i];
                Console.WriteLine($"Text: {row.Text}");
                Console.WriteLine($"LDA Features: {string.Join(", ", row.LdaFeatures)}");
                Console.WriteLine();
            }

            return transformedDataEnumerable;
        }

        private string PreprocessText(string text)
        {
            return new string(text.Where(c => !char.IsPunctuation(c)).ToArray()).ToLower();
        }

        public class TextData
        {
            public string Text { get; set; }
        }

        public class TransformedTextData : TextData
        {
            [VectorType(5)]
            public float[] LdaFeatures { get; set; }
        }
    }
}
