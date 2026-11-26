export const useMediaQuery = (breakpoint: number) => {
  const [isMobile, setIsMobile] = useState<boolean>(false);
  
  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= breakpoint);
    };
    
    // Set initial value
    setIsMobile(window.innerWidth <= breakpoint);
    
    // Add event listener
    window.addEventListener('resize', handleResize);
    
    // Cleanup
    return () => window.removeEventListener('resize', handleResize);
  }, [breakpoint]);
  
  return isMobile;
};