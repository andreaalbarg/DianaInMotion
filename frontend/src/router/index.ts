import { createRouter, createWebHistory } from 'vue-router'
import { useAdminAuthStore } from '../stores/adminAuth'

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
      ],
    },
  ],
})

router.beforeEach((to, _from, next) => {
  if (to.meta.requiresAuth) {
    const auth = useAdminAuthStore()
    if (!auth.isAuthenticated) {
      next('/admin/login')
      return
    }
  }
  next()
})

export default router
