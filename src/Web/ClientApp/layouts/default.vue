<template>
  <header>
    <nav>
      <ul>
        <li><NuxtLink to="/"><strong>Lift &amp; Shift</strong></NuxtLink></li>
      </ul>
      <ul>
        <NavMenu />
        <li aria-hidden="true" class="nav-separator"></li>
        <li><ThemeToggle /></li>
      </ul>
    </nav>
  </header>
  <slot />
</template>
