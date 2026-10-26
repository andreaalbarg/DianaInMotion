import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: () => import('../App.vue'),
    },
    {
      path: '/admin/login',
      component: () => import('../views/admin/AdminLogin.vue'),
    },
    {
      path: '/admin',
      component: () => import('../views/admin/AdminLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        { path: '', redirect: '/admin/events' },
        { path: 'events', component: () => import('../views/admin/EventsView.vue') },
        { path: 'registrations', component: () => import('../views/admin/RegistrationsView.vue') },
        { path: 'subscribers', component: () => import('../views/admin/SubscribersView.vue') },
        { path: 'youtube', component: () => import('../views/admin/YouTubeView.vue') },
        { path: 'email-test', component: () => import('../views/admin/EmailTestView.vue') },
      ],
    },
  ],
})

// Read directly from localStorage — avoids Pinia initialization race in the guard
router.beforeEach((to, _from, next) => {
  if (to.meta.requiresAuth && !localStorage.getItem('admin_token')) {
    next('/admin/login')
    return
  }
  next()
})

export default router
