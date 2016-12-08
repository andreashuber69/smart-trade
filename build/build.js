var browserify = require('browserify');
var tsify = require('tsify');
var exorcist = require('exorcist');
var fs = require('fs');
var path = require('path');

var outputDir = path.join(process.cwd(), 'www/js');

if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir);
}

var scriptFile = path.join(outputDir, "index.js");
var mapFile = scriptFile + ".map";

browserify({ debug: true })
    .add('scripts/index.ts')
    .plugin(tsify)
    .bundle()
    .pipe(exorcist(mapFile, '', process.cwd(), ''))
    .pipe(fs.createWriteStream(scriptFile, 'utf8'));