window.component.Category = function (data) {
  let category = ce({ className: 'category' })

  let files = {}

  category.files = files

  for (let key in data) {
    if (!files[data[key][0].texture]) {
      let file = ce({ className: 'file' })
      file.texture = data[key][0].texture

      file.append(file.header = ce({ name: 'header', innerHTML: file.texture }))
      file.append(file.main = ce({ name: 'main' }))

      file.header.addEventListener('click', (e) => {
        let entities = Array.from(file.querySelectorAll('.entity'))
        let state = !entities[0].Enabled
        for (let e of entities) e.Enabled = state
      })

      files[data[key][0].texture] = file
    }
  }

  for (let name in files) {
    category.append(files[name])
  }

  category.entities = {}

  for (let key in data) {
    let entity = new component.Entity(key, data[key])
    files[data[key][0].texture].main.append(entity)
    category.entities[key] = entity
  }

  return category
}

window.component.File = function () {

}


window.component.App = function () {
  let app = ce({ id: 'app' })
  window.app = app


  app.append(app.header = new component.Header())
  app.append(app.main = new component.Main())


  app.categories = {}

  Object.defineProperty(app, "Category", {
    get() { return app.category },
    set(name) {
      app.category = name
      app.main.set(app.categories[name])
    },
  });



  for (let category in all) {
    app.categories[category] = new component.Category(all[category])

    app.header.addButton(category)
  }

  app.Category = 'structures'

  app.save = () => {
    let blacklist = {}

    for (let category in app.categories) {
      blacklist[category] = {}

      for (let id in app.categories[category].entities) {
        blacklist[category][id] = app.categories[category].entities[id].Enabled
      }
    }

    saveFile(blacklist, "Entity Blacklist")
  }

  app.load = async () => {
    let blacklist = await loadFile()

    if (typeof Object.values(blacklist)[0] == 'object') {
      // it's new

      for (let category in blacklist) {
        for (let id in blacklist[category]) {
          app.categories[category].entities[id].Enabled = blacklist[category][id]
        }
      }
    } else {
      // it's old

      for (let id in blacklist) {
        for (let category in app.categories) {
          if (app.categories[category].entities[id]) {
            app.categories[category].entities[id].Enabled = blacklist[id]
            break
          }
        }
      }
    }
  }

  app.merge = async () => {
    let blacklist = await loadFile()

    for (let category in blacklist) {
      for (let id in blacklist[category]) {
        if (!blacklist[category][id]) app.categories[category].entities[id].Enabled = false
      }
    }

    if (typeof Object.values(blacklist)[0] == 'object') {
      // it's new

      for (let category in blacklist) {
        for (let id in blacklist[category]) {
          if (blacklist[category][id] == false) {
            app.categories[category].entities[id].Enabled = false
          }
        }
      }
    } else {
      // it's old

      for (let id in blacklist) {
        for (let category in app.categories) {
          if (app.categories[category].entities[id]) {
            if (blacklist[id] == false) {
              app.categories[category].entities[id].Enabled = blacklist[id]
            }

            break
          }
        }
      }
    }
  }



  return app
}