const express = require('express')
var cfg = require('./config');
var bodyParser = require('body-parser');
var connectionFactory = require('./connectionFactory');
var sender = require('./messageSender');
var validation = require('./validation');
var appParams = require('./appParameters');
var eventDataMapper = require('./eventdata-mapper');
var elasticRepositoryModule = require('./ElasticRepository');
var elasticRepository = new elasticRepositoryModule("diaryEvent", "diary-events");

// logger setup
var log4js = require('log4js');
var loggerConfig = require('./logging/');
loggerConfig.run();
var logger = log4js.getLogger('app');

const app = express();
app.use(bodyParser.json());

var appParamsInstance = new appParams(process.argv);
var appPort = appParamsInstance.getPort(cfg.port);
var connFactoryInstance = new connectionFactory();
var conn = connFactoryInstance.CreateEsConnection();
var sender = new sender(conn);

app.use(function (req, res, next) {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    next();
});

app.get('/', function (req, res) {
    res.status(200).send('ok');
});

app.get('/api/v1/diary/securitylink/:profile', function (req, res) {
    // TODO return diary id
    var id = eventDataMapper.getCorrelationId(req.params.profile);

    // TODO query elastic and get the securityLink
    // test this!!
    elasticRepository.GetById(id)
        .then(d => {
            res.send(d);
        }).catch(err => {
            logger.error(err);
        });
});

app.post('/api/v1/diary', function (req, res) {
    if (validation.requestNotValid(req)) {
        var errorMessage = "Request body not valid";
        logger.error(errorMessage);
        return res.status(400).send(errorMessage);
    }
    sender.send(req.body).then(function (result) {
        logger.debug("Event stored");
        res.status(201).send(); // TODO redirect on the diary?
    })
        .catch(function (err) {
            logger.error(err);
            res.status(500).send('There is a technical problem and the log has not been stored');
        });
})

var server = app.listen(appPort, cfg.host, function () {
    var address = server.address().address + ":" + server.address().port;

    logger.info("Running environment configuration: %s", cfg.env)
    logger.info(cfg.projectName + " listening on %s", address)
    logger.info(cfg.projectName + " publishing to %s", cfg.publishTo)
});
module.exports = server;