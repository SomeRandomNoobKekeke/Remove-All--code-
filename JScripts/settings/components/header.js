window.component.Header = function () {
  let header = ce({ name: 'header', id: 'header' })

  header.append(header.nav = ce({ name: 'nav' }))

  header.addButton = (category) => {
    let button = ce({ name: 'button', innerHTML: category })
    button.addEventListener('click', (e) => {
      app.Category = category
    })
    header.nav.append(button)
  }

  header.append(header.menu = ce({ className: 'menu' }))
  header.menu.append(ce({ name: 'button', innerHTML: 'save', onclick: () => app.save() }))
  header.menu.append(ce({ name: 'button', innerHTML: 'load', onclick: () => app.load() }))
  header.menu.append(ce({ name: 'button', innerHTML: 'merge with', onclick: () => app.merge() }))

  return header
}