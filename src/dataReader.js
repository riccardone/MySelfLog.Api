var moment = require("moment");

module.exports = function(client) {
  function ping() {
    client.ping(
      {
        // ping usually has a 3000ms timeout
        requestTimeout: 1000
      },
      function(error) {
        if (error) {
          console.trace("elasticsearch cluster is down!");
        } else {
          console.log("All is well");
        }
      }
    );
  }

  function search(correlationId, logType, from, to) {
    return client.search({
      index: "diary-logs",
      type: "diaryLog",
      size: 10000,
      q:
        'CorrelationId: "' +
        correlationId +
        '" AND LogType: "' +
        logType +
        '" AND LogDate: ["' +
        from.toISOString() +
        '" TO "' +
        to.toISOString() +
        '"]'
    });
  }

  async function getData(diaryName, logType, from, to) {
    const diaryEventResponse = await client.search({
      index: "diary-events",
      type: "diaryEvent",
      size: 10000,
      q: 'DiaryName: "' + diaryName + '"'
    });
    // TODO check if is an error or if there are hits before do that
    if (diaryEventResponse.hits.total === 0) {
      throw { message: "Diary '" + diaryName + "' not found" };
    }
    var correlationId = diaryEventResponse.hits.hits[0]._source.Id;
    const diaryLogResponse = await search(correlationId, logType, from, to);
    var results = [];
    diaryLogResponse.hits.hits.forEach(element => {
      results.push({
        Id: element._source.Id,
        Value: element._source.Value,
        Mmolvalue: element._source.Mmolvalue,
        SlowTerapy: element._source.SlowTerapy,
        FastTerapy: element._source.FastTerapy,
        Calories: element._source.Calories,
        Message: element._source.Message,
        LogType: element._source.LogType,
        LogDate: element._source.LogDate,
        Source: element._source.Source,
        CorrelationId: element._source.CorrelationId
      });
    });
    return results;
  }

  async function getValues(diaryName, format, from, to) {
    const getDataResponse = await getData(diaryName, "Blood-Sugar", from, to);
    var data = [];
    getDataResponse.forEach(element => {
      data.push({
        Value: getValue(element),
        Message: element.Message,
        LogDate: moment(element.LogDate).format("YYYY-MM-DD HH:mm:ss")
      });
    });
    function getValue(el_1) {
      if (format === "mmol") {
        return el_1.Mmolvalue;
      } else {
        return el_1.Value;
      }
    }
    return data;
  }

  async function getFastTerapies(diaryName, from, to) {
    const getDataResponse = await getData(diaryName, "Fast-Terapy", from, to);
    var data = [];
    getDataResponse.forEach(element => {
      data.push({
        Fast: element.FastTerapy,
        Message: element.Message,
        LogDate: moment(element.LogDate).format("YYYY-MM-DD HH:mm:ss")
      });
    });
    return data;
  }

  async function getSlowTerapies(diaryName, from, to) {
    const getDataResponse = await getData(diaryName, "Slow-Terapy", from, to);
    var data = [];
    getDataResponse.forEach(element => {
      data.push({
        Slow: element.SlowTerapy,
        Message: element.Message,
        LogDate: moment(element.LogDate).format("YYYY-MM-DD HH:mm:ss")
      });
    });
    return data;
  }

  async function getTerapies(diaryName, from, to) {
    var data = [];
    const slowTerapies = await getData(diaryName, "Slow-Terapy", from, to);
    slowTerapies.forEach(element => {
      data.push({
        Slow: element.SlowTerapy,
        Message: element.Message,
        LogDate: moment(element.LogDate).format("YYYY-MM-DD HH:mm:ss")
      });
    });
    data;
    const fastTerapies = await getData(diaryName, "Fast-Terapy", from, to);
    fastTerapies.forEach(element => {
      data.push({
        Fast: element.FastTerapy,
        Message: element.Message,
        LogDate: moment(element.LogDate).format("YYYY-MM-DD HH:mm:ss")
      });
    });
    return data;
  }

  async function getCalories(diaryName, from, to) {
    const getDataResponse = await getData(diaryName, "Calories", from, to);
    var data = [];
    getDataResponse.forEach(element => {
      data.push({
        Calories: element.Calories,
        Message: element.Message,
        LogDate: moment(element.LogDate).format("YYYY-MM-DD HH:mm:ss")
      });
    });
    return data;
  }

  return {
    ping: ping,
    getData: getData,
    getValues: getValues,
    getFastTerapies: getFastTerapies,
    getSlowTerapies: getSlowTerapies,
    getTerapies: getTerapies,
    getCalories: getCalories
  };
};
