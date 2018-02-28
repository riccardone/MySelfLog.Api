const express = require('express')
// var swaggerUi = require('swagger-ui-express');
// var swaggerDocument = require('./swagger.json');
// https://blog.cloudboost.io/adding-swagger-to-existing-node-js-project-92a6624b855b
var bodyParser = require('body-parser');
var validation = require('./validation');
var eventDataMapper = require('./eventdata-mapper');

const app = express();
app.use(bodyParser.json());

var _sender;
var _publishTo = "diary-input";
var _esClient;
var _logger;

function Service(sender, esClient, logger) {   
    _sender = sender;
    _esClient = esClient;
    _logger = logger;
}

app.use(function (req, res, next) {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    next();
});

app.get('/', function (req, res) {
    res.send('ok');
});

app.get('/api/v1/diary/check/:diaryName', function (req, res) {
    // Query elastic and check if this diaryName is already in use   
    var diaryName = req.params.diaryName;
    _esClient.search({
        index: "diary-events",
        q: 'DiaryName:"' + diaryName + '"'
    }).then(d => {
        if (d.hits.total > 0) {
            return res.status(200).send("not available");
        }
        return res.status(200).send("available");
    }).catch(err => {
        if (err.message.startsWith("[index_not_found_exception]")) {
            return res.status(200).send("available");
        }
        if (!err.statusCode) {
            res.status(500).send('error');
        } else {
            res.status(err.statusCode).send('error');
        }
        _logger.error(err);
    });
});

app.get('/api/v1/diary/:profile', function (req, res) {
    var id = eventDataMapper.getCorrelationId(req.params.profile);
    // Query elastic and check if this diaryName is already in use       
    _esClient.get({
        index: "diary-events",
        type: "diaryEvent",
        id: id
    }).then(d => {
        if (d.found) {
            return res.status(200).send(d._source);
        }
        return res.status(404).send();
    }).catch(err => {
        if (err.message.startsWith("[index_not_found_exception]")) {
            return res.status(err.statusCode).send("index not found");
        }
        if (!err.statusCode) {
            res.status(500).send('error');
        } else {
            res.status(err.statusCode).send('error');
        }
        _logger.error(err);
    });
});

app.post('/api/v1/logs', function (req, res) {
    if (validation.requestNotValid(req)) {
        var errorMessage = "Request body not valid";
        _logger.error(errorMessage);
        return res.status(400).send(errorMessage);
    }
    _sender.send(req.body, 'LogReceived').then(function (result) {
        _logger.debug("Event stored");
        res.send(); // TODO redirect on the diary?
    }).catch(function (err) {
        _logger.error(err);
        res.status(500).send('There is a technical problem and the log has not been stored');
    });
})

app.post('/api/v1/diary', function (req, res) {
    if (validation.requestNotValid(req)) {
        var errorMessage = "Request body not valid";
        _logger.error(errorMessage);
        return res.status(400).send(errorMessage);
    }
    // TODO change this nonsense event type and use a proper one
    _sender.send(req.body, 'CreateDiary').then(function (result) {
        _logger.debug("Event stored");
        res.send(); // TODO redirect on the diary?
    }).catch(function (err) {
        _logger.error(err);
        res.status(500).send('There is a technical problem and the log has not been stored');
    });
})

var server = app.listen(5001, '0.0.0.0', function () {
    var address = server.address().address + ":" + server.address().port;
    
    _logger.info("MySelfLog-Api listening on %s", address)
    _logger.info("MySelfLog-Api publishing to %s", _publishTo)
});

Service.prototype.server = server;
module.exports = Service;