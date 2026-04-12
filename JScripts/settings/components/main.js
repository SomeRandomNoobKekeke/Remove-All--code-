window.component.Main = function () {
  let main = ce({ name: 'main', innerHTML: 'main' })
  main.set = (content) => {
    main.innerHTML = ''
    main.append(content)
  }

  return main
}