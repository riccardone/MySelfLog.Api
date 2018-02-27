var connectionFactory = require('./connectionFactory');
var senderModule = require('./messageSender');
var elasticsearch = require('elasticsearch');
var serviceModule = require('./service');

// logger setup
var log4js = require('log4js');
var loggerConfig = require('./logging/');
loggerConfig.run();
var logger = log4js.getLogger('app');

const app = express();
app.use(bodyParser.json());

var connFactoryInstance = new connectionFactory();
var conn = connFactoryInstance.CreateEsConnection();
var sender = new senderModule(conn);

var service = new serviceModule(sender, getElasticClient());

function getElasticClient() {
    return new elasticsearch.Client({
        host: 'http://elasticsearch:9200/',
        log: 'trace'
    });
}