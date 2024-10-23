function ce({ name = 'div', innerHTML = "", className = "", id = '', onclick = null }) {
  let d = document.createElement(name)
  if (innerHTML) d.innerHTML = innerHTML
  if (className) d.className = className
  if (id) d.id = id
  if (onclick) d.onclick = onclick
  return d
}

function main() {
  document.body.append(new component.App())
}

window.component = {}

window.mouse = {
  pressed: false,
  enabling: false,
}

document.addEventListener('mouseup', (e) => {
  mouse.pressed = false
})

document.addEventListener('keydown', (e) => {
  if (e.code == 'Space') {
    e.preventDefault()
    app.save()
  }
})

window.contentFolder = "C:/Program Files (x86)/Steam/steamapps/common/Barotrauma/Content/"


// took this from previos version, have no idea how it works and won't touch it
if (location.href.lastIndexOf('Barotrauma') != -1) {
  let p = location.href.slice(0, location.href.lastIndexOf('/'))
  pathToBarotrauma = location.href.slice(0, p.lastIndexOf('Barotrauma') + 'Barotrauma'.length) + '/'

  window.contentFolder = pathToBarotrauma + 'Content/'
} else {
  pathToBarotrauma = `file:///` + prompt('where is barotrauma folder?', `C:/Program Files (x86)/Steam/steamapps/common/Barotrauma/`)

  window.contentFolder = pathToBarotrauma + 'Content/'
}

window.loadFile = async () => {
  return new Promise(resolve => {
    let input = ce({ name: 'input' });
    input.type = 'file';

    input.onchange = e => {
      let file = e.target.files[0]
      if (!file) return

      let reader = new FileReader()
      reader.onload = function (e) {
        resolve(JSON.parse(e.target.result))
      }
      reader.readAsText(file)
    }

    input.click();
  })
}

window.saveFile = (data, fileName) => {
  let link = document.createElement("a");
  let file = new Blob([JSON.stringify(data, null, 2)], { type: 'text/plain' });
  link.href = URL.createObjectURL(file);
  link.download = `${fileName}.json`;
  link.click();
  URL.revokeObjectURL(link.href)
}