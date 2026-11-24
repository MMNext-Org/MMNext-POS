-- Add function to set session context variables after authentication
CREATE OR REPLACE FUNCTION public.set_app_context(
    org_id   uuid,
    branch_id uuid,
    role_id  uuid,
    role_name text
) RETURNS void LANGUAGE plpgsql AS $$
BEGIN
    SET LOCAL app.current_organization_id = org_id;
    SET LOCAL app.current_branch_id      = branch_id;
    SET LOCAL app.current_role_id        = role_id;
    SET LOCAL app.current_role_name      = role_name;
END;
$$;