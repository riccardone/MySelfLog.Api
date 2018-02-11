require('dotenv').config();
const express = require('express')
// var swaggerUi = require('swagger-ui-express');
// var swaggerDocument = require('./swagger.json');
// https://blog.cloudboost.io/adding-swagger-to-existing-node-js-project-92a6624b855b
var cfg = require('./config');
var bodyParser = require('body-parser');
var connectionFactory = require('./connectionFactory');
var senderModule = require('./messageSender');
var validation = require('./validation');
var eventDataMapper = require('./eventdata-mapper');
var elasticsearch = require('elasticsearch');

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

app.use(function (req, res, next) {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    next();
});

app.get('/', function (req, res) {
    res.send('ok');
});

function getElasticClient(){
    return new elasticsearch.Client({
        host: cfg.elasticSearchLink,
        log: 'trace'
    });
}

app.get('/api/v1/diary/check/:diaryName', function (req, res) {
    // Query elastic and check if this diaryName is already in use   
    var diaryName = req.params.diaryName;
    getElasticClient().search({
        index: "diary-events",
        q: 'DiaryName:"' + diaryName + '"'
    }).then(d => {
        if (d.hits.total > 0) {
            return res.status(201).send("not available");
        }
        return res.status(201).send("available");
    }).catch(err => {
        if (!err.statusCode) {
            res.status(500).send('error');
        } else {
            res.status(err.statusCode).send('error');
        }
        logger.error(err);
    });
});

app.get('/api/v1/diary/:profile', function (req, res) {
    var id = eventDataMapper.getCorrelationId(req.params.profile);
    // Query elastic and check if this diaryName is already in use   
    var diaryName = req.params.diaryName;
    getElasticClient().get({
        index: "diary-events",
        type: "diaryEvent",
        id: id
    }).then(d => {
        if (d.found) {            
            return res.status(201).send(d._source);    
        }
        return res.status(201).send("not available");
    }).catch(err => {
        if (!err.statusCode) {
            res.status(500).send('error');
        } else {
            res.status(err.statusCode).send('error');
        }
        logger.error(err);
    });
});

app.post('/api/v1/logs', function (req, res) {
    if (validation.requestNotValid(req)) {
        var errorMessage = "Request body not valid";
        logger.error(errorMessage);
        return res.status(400).send(errorMessage);
    }
    sender.send(req.body, 'LogReceived').then(function (result) {
        logger.debug("Event stored");
        res.send(); // TODO redirect on the diary?
    }).catch(function (err) {
        logger.error(err);
        res.status(500).send('There is a technical problem and the log has not been stored');
    });
})

app.post('/api/v1/diary', function (req, res) {
    if (validation.requestNotValid(req)) {
        var errorMessage = "Request body not valid";
        logger.error(errorMessage);
        return res.status(400).send(errorMessage);
    }
    // TODO change this nonsense event type and use a proper one
    sender.send(req.body, 'CreateDiary').then(function (result) {
        logger.debug("Event stored");
        res.send(); // TODO redirect on the diary?
    }).catch(function (err) {
        logger.error(err);
        res.status(500).send('There is a technical problem and the log has not been stored');
    });
})

var server = app.listen(cfg.port, cfg.host, function () {
    var address = server.address().address + ":" + server.address().port;

    logger.info("Running environment configuration: %s", cfg.env)
    logger.info(cfg.projectName + " listening on %s", address)
    logger.info(cfg.projectName + " publishing to %s", cfg.publishTo)
});
module.exports = server;