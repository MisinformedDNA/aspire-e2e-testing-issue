window.clerkInterop = {
    dotNetRef: null,

    init: function (dotNetRef) {
        this.dotNetRef = dotNetRef;
        if (typeof Clerk !== 'undefined' && Clerk.addListener) {
            Clerk.addListener(({ user }) => {
                const userInfo = user ? {
                    userId: user.id,
                    email: user.primaryEmailAddress?.emailAddress ?? null,
                    displayName: user.fullName ?? user.firstName ?? null
                } : null;
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnAuthStateChanged', userInfo);
                }
            });
        }
    },

    getUserInfo: async function () {
        if (typeof Clerk === 'undefined' || !Clerk.user) return null;
        const user = Clerk.user;
        return {
            userId: user.id,
            email: user.primaryEmailAddress?.emailAddress ?? null,
            displayName: user.fullName ?? user.firstName ?? null
        };
    },

    openSignIn: async function () {
        if (typeof Clerk !== 'undefined') {
            await Clerk.openSignIn();
        }
    },

    signOut: async function () {
        if (typeof Clerk !== 'undefined') {
            await Clerk.signOut();
        }
    },

    getToken: async function () {
        if (typeof Clerk === 'undefined' || !Clerk.session) return null;
        return await Clerk.session.getToken();
    }
};
