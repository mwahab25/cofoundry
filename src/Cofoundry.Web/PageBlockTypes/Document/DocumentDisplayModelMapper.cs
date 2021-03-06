﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cofoundry.Core;
using Cofoundry.Domain;
using Cofoundry.Domain.CQS;
using System.Threading.Tasks;

namespace Cofoundry.Web
{
    public class DocumentDisplayModelMapper : IPageBlockTypeDisplayModelMapper<DocumentDataModel>
    {
        #region Constructor

        private IQueryExecutor _queryExecutor;
        private IDocumentAssetRouteLibrary _documentAssetRouteLibrary;

        public DocumentDisplayModelMapper(
            IQueryExecutor queryExecutor,
            IDocumentAssetRouteLibrary documentAssetRouteLibrary
            )
        {
            _queryExecutor = queryExecutor;
            _documentAssetRouteLibrary = documentAssetRouteLibrary;
        }

        #endregion

        public async Task<IEnumerable<PageBlockTypeDisplayModelMapperOutput>> MapAsync(
            IReadOnlyCollection<PageBlockTypeDisplayModelMapperInput<DocumentDataModel>> inputCollection, 
            PublishStatusQuery publishStatusQuery
            )
        {
            var documentIds = inputCollection.SelectDistinctModelValuesWithoutEmpty(i => i.DocumentAssetId);
            var documentsQuery = new GetDocumentAssetRenderDetailsByIdRangeQuery(documentIds);
            var documents = await _queryExecutor.ExecuteAsync(documentsQuery);

            var results = new List<PageBlockTypeDisplayModelMapperOutput>(inputCollection.Count);

            foreach (var input in inputCollection)
            {
                var document = documents.GetOrDefault(input.DataModel.DocumentAssetId);

                var output = new DocumentDisplayModel();
                if (document != null)
                {
                    output.Description = document.Description;
                    output.Title = document.Title;
                    if (input.DataModel.DownloadMode == DocumentDownloadMode.ForceDownload)
                    {
                        output.Url = _documentAssetRouteLibrary.DocumentAssetDownload(document);
                    }
                    else
                    {
                        output.Url = _documentAssetRouteLibrary.DocumentAsset(document);
                    }
                }

                results.Add(input.CreateOutput(output));
            }

            return results;
        }
    }
}