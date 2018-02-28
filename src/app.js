var ConnectionFactory = require('./connectionFactory')
var SenderModule = require('./messageSender')
var Elasticsearch = require('elasticsearch')
var ServiceModule = require('./service')

// logger setup
var log4js = require('log4js')
var loggerConfig = require('./logging/')
loggerConfig.run()

var connFactoryInstance = new ConnectionFactory()
var conn = connFactoryInstance.CreateEsConnection()
var sender = new SenderModule(conn)

var service = new ServiceModule(sender, getElasticClient(), log4js.getLogger('service'))

function getElasticClient () {
  return new Elasticsearch.Client({
    host: 'http://elasticsearch:9200/',
    log: 'trace'
  })
}
