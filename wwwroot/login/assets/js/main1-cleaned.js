
// Initialize AOS if available
document.addEventListener('DOMContentLoaded', () => {
  if (typeof AOS !== 'undefined') {
    AOS.init({
      duration: 800,
      easing: 'slide',
      once: true
    });
  }

  // Scroll to top functionality if scroll-top button exists
  const scrollTop = document.querySelector('.scroll-top');
  if (scrollTop) {
    scrollTop.addEventListener('click', (e) => {
      e.preventDefault();
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    });
  }

  // Handle scroll event to toggle 'scrolled' class
  const header = document.querySelector('#header');
  const body = document.body;

  if (header && body) {
    window.addEventListener('scroll', () => {
      if (!header.classList.contains('scroll-up-sticky') &&
          !header.classList.contains('sticky-top') &&
          !header.classList.contains('fixed-top')) return;

      window.scrollY > 100
        ? body.classList.add('scrolled')
        : body.classList.remove('scrolled');
    });
  }
});
