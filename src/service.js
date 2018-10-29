module.exports = function(config) {
  const express = require("express");
  var cors = require("cors");
  var bodyParser = require("body-parser");
  var sender = (config && config.sender) || eventStoreSender(config);
  var esClient = (config && config.esClient) || elasticClient();
  var reader = require("./dataReader");
  var diaryRoute = require("./routes/api.v1.diary")(
    sender,
    esClient,
    config.logger,
    reader(esClient)
  );
  var logsRoute = require("./routes/api.v1.logs")(
    sender,
    esClient,
    config.logger
  );
  const app = express();
  app.use(cors());
  app.use(bodyParser.json());

  app.get("/", function(req, res) {
    res.send("ok");
  });

  app.use("/api/v1/diary", diaryRoute);
  app.use("/api/v1/logs", logsRoute);

  return app;
};

function eventStoreSender(config) {
  var conn = require("./connectionFactory")(config.logger).CreateEsConnection();
  return require("./messageSender")(conn, config.publishTo);
}

function elasticClient() {
  var Elasticsearch = require("elasticsearch");
  return new Elasticsearch.Client({
    host: "http://elasticsearch:9200/"
  });
}
