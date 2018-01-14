var elasticsearch = require('elasticsearch');

var _docType, _indexName;

function ElasticRepository(docType, indexName) {
    this._docType = docType;
    this._indexName = indexName;
}

ElasticRepository.prototype.GetById = function (id) {
    return _client.get({
        index: this._indexName,
        type: this._docType,
        id: id
    });
}

module.exports = ElasticRepository;