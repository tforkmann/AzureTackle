module.exports = {
    daisyui : {
        themes: true
    },
    content: [
        "./src/docs/.fable-build/**/*.{js,ts,jsx,tsx}",
    ],
    plugins: [
        require('daisyui'),
    ]
}
