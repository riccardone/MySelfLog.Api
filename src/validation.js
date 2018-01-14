module.exports = {
    requestNotValid: function (req) {
        if (isEmptyObject(req.body))
            return true;
        if (!req.body.applies)
            return true;
        if (!req.body.source)
            return true;
        if (!req.body.profile)
            return true;
        return false;
    }
};

function isEmptyObject(obj) {
    return !Object.keys(obj).length;
}