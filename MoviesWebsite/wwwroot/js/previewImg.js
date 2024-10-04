document.addEventListener("DOMContentLoaded", function () {
  document.getElementById("image").addEventListener("change", function (event) {
    const [file] = event.target.files;
    const preview = document.getElementById("selectedImagePreview");

    if (file) {
      preview.src = URL.createObjectURL(file);
      preview.style.display = "block";
    } else {
      preview.style.display = "none";
    }
  });
});
