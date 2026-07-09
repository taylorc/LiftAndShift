<template>
  <header>
    <nav>
      <ul>
        <li><NuxtLink to="/">Lift &amp; Shift</NuxtLink></li>
      </ul>
      <ul>
        <li><NuxtLink to="/">Home</NuxtLink></li>
        <li><NuxtLink to="/counter">Counter</NuxtLink></li>
        <li><NuxtLink to="/weather">Weather</NuxtLink></li>
        <li><NuxtLink to="/todo">Tasks</NuxtLink></li>
      </ul>
      <ul>
        <NavMenu />
        <li aria-hidden="true" class="nav-separator"></li>
        <li><ThemeToggle /></li>
      </ul>
    </nav>
  </header>
  <main>
    <slot />
  </main>
</template>
