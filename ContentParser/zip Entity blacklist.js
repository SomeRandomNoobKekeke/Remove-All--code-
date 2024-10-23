let fs = require('fs')

let folder = 'settings/'

let raw = fs.readFileSync(folder + 'settings.html', 'utf8')

let css = Array.from(raw.matchAll(/  <link rel="stylesheet" href="(.*)">/g))
let js = Array.from(raw.matchAll(/  <script src="(.*)"><\/script>/g))

for (let match of css) {
  console.log(match[0], folder + match[1]);
  raw = raw.replace(match[0],
    `<style>\n${fs.readFileSync(folder + match[1], 'utf8')}\n</style>`
  )
}

for (let match of js) {
  console.log(match[0], folder + match[1]);
  raw = raw.replace(match[0],
    `<script>\n${fs.readFileSync(folder + match[1], 'utf8')}\n</script>`
  )
}

fs.writeFileSync('Entity Blacklist.html', raw)




