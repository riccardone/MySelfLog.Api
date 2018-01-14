const log4js = require('log4js');
var cfg = require('../config');

module.exports = {
    run : function(){
        log4js.configure({
            appenders:{
                debug : { type: 'console', category: 'debug' },
                info : {
                    type: 'file', 
                    filename: './logs/' + cfg.projectName + '.log', 
                    maxLogSize: 10485760, 
                    backups: 5, 
                    compress: true, 
                    category: 'info' 
                },
                ci : { 
                    type: 'stdout', 
                    category: 'info' 
                },
                error : {
                    type: 'file', 
                    filename: './logs/' + cfg.projectName + '.log', 
                    maxLogSize: 10485760, 
                    backups: 5, 
                    compress: true, 
                    category: 'error' 
                },
            },
            categories: 
            { 
                default: { 
                    appenders: 
                        [cfg.logAppender], 
                        level: cfg.logLevel 
                } 
            }
        });
    }
};