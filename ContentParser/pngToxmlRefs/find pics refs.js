let fs = require('fs')
let path = require('path')



let barotraumaFolder = "C:/Program Files (x86)/Steam/steamapps/common/Barotrauma"
let contentFolder = "C:/Program Files (x86)/Steam/steamapps/common/Barotrauma/Content"



let xmlFiles = recFindByExt(contentFolder, 'xml').map(f => ({
  full: f,
  rel: path.relative(barotraumaFolder, f),
  dir: path.relative(barotraumaFolder, path.dirname(f)),
}))

let pngFiles = recFindByExt(contentFolder, 'png').map(f => [path.relative(barotraumaFolder, f), true])
pngFiles = Object.fromEntries(pngFiles)


for (let xml of xmlFiles) {
  findRefs(xml)
}

xmlFiles = xmlFiles.filter(xml => xml.refs.length != 0)


xmlFiles.forEach(xml => {
  xml.refs = xml.refs.map(r => path.dirname(r) == '.' ? path.join(xml.dir, r) : r.replace(/\//g, '\\'))
  xml.refs = xml.refs.filter(r => pngFiles[r])
})


let inv = {}
for (let xml of xmlFiles) {
  for (let ref of xml.refs) {
    if (!inv[ref]) inv[ref] = []
    inv[ref].push(xml.rel)
  }
}

fs.writeFileSync('pngToXml.js', `var pngToXml = ${JSON.stringify(inv, null, 4)}`)




console.log(findRefs(xmlFiles[1]));


function findRefs(xml) {
  let text = fs.readFileSync(xml.full, 'utf8')
  let matches = text.match(/"(\w+\/)*\w+\.png/g) || []

  matches = matches.map(m => m.slice(1))

  let refs = {}
  for (let m of matches) {
    refs[m] = true
  }

  refs = Object.keys(refs)

  xml.refs = refs

  return refs
}


function recFindByExt(base, ext, files, result) {
  files = files || fs.readdirSync(base)
  result = result || []

  files.forEach(
    function (file) {
      var newbase = path.join(base, file)
      if (fs.statSync(newbase).isDirectory()) {
        result = recFindByExt(newbase, ext, fs.readdirSync(newbase), result)
      }
      else {
        if (file.substr(-1 * (ext.length + 1)) == '.' + ext) {
          result.push(newbase)
        }
      }
    }
  )
  return result
}









