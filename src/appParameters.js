module.exports = AppParameters;

const portIndex = 2;
var _appCustomPort;

function AppParameters(args) {
    this._appCustomPort = args[portIndex]
}

AppParameters.prototype.getPort = function(portFromConfig) {
    if(this._appCustomPort == undefined){
        return portFromConfig;
    }

    return parseInt(this._appCustomPort);
}